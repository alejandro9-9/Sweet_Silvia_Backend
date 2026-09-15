using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Auth;
using Sweet_Silvia_Application.RefreshToken;

namespace Sweet_Silvia_Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender, IWebHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    private const string RefreshTokenCookieName = "sweet-silvia-refresh-token";

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
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
    [EnableRateLimiting("auth")]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var customerRoleId = configuration.GetValue<Guid>("Roles:CustomerRoleId");
        if (customerRoleId == Guid.Empty)
        {
            return Problem(
                title: "Customer role is not configured.",
                detail: "Configure Roles:CustomerRoleId before allowing Google registration.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await sender.Send(
            new GoogleLoginCommand(
                request.IdToken ?? request.Credential,
                request.AuthorizationCode,
                customerRoleId,
                request.Name,
                request.PaternalSurname,
                request.MaternalSurname,
                request.Phone,
                request.DeviceInfo ?? userAgent,
                GetIpAddress(),
                userAgent),
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAt);
        return Ok(new { token = result.Value.Token });
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!IsTrustedOrigin())
        {
            return Forbid();
        }

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
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private bool IsTrustedOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (environment.IsDevelopment())
        {
            return string.IsNullOrWhiteSpace(origin) ||
                   allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
                   Uri.TryCreate(origin, UriKind.Absolute, out var developmentOrigin) && developmentOrigin.IsLoopback;
        }

        return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record GoogleLoginRequest(
    string? IdToken = null,
    string? Credential = null,
    string? AuthorizationCode = null,
    string? Name = null,
    string? PaternalSurname = null,
    string? MaternalSurname = null,
    string? Phone = null,
    string? DeviceInfo = null);
