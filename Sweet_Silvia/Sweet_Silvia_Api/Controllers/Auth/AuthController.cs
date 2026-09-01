using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.RefreshToken;

namespace Sweet_Silvia_Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender, IWebHostEnvironment environment) : ControllerBase
{
    private const string RefreshTokenCookieName = "sweet-silvia-refresh-token";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(request.Email, request.Password, Request.Headers.UserAgent.ToString(), GetIpAddress(), Request.Headers.UserAgent.ToString()),
            cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        return Ok(new { token = result.Value.Token });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result = await sender.Send(
            new RefreshSessionCommand(refreshToken, Request.Headers.UserAgent.ToString(), GetIpAddress(), Request.Headers.UserAgent.ToString()),
            cancellationToken);

        if (result.IsFailure)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(result.Error);
        }

        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        return Ok(new { token = result.Value.Token });
    }

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            await sender.Send(new RevokeSessionCommand(refreshToken, GetIpAddress(), Request.Headers.UserAgent.ToString()), cancellationToken);
        }

        DeleteRefreshTokenCookie();
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, CreateCookieOptions(expiresAt));
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, CreateCookieOptions());
    }

    private CookieOptions CreateCookieOptions(DateTimeOffset? expiresAt = null)
    {
        var isProduction = !environment.IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = expiresAt,
            IsEssential = true,
            Path = "/api/auth"
        };
    }

    private string? GetIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

public sealed record LoginRequest(string Email, string Password);
