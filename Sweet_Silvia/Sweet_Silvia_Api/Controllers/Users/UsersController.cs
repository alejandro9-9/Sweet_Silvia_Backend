using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.RefreshToken;
using Sweet_Silvia_Application.User;
using Sweet_Silvia_Domain.User;

namespace Sweet_Silvia_Api.Controllers.Users;

[ApiController]
[Route("api/users")]
public sealed class UsersController(ISender sender, IConfiguration configuration) : ControllerBase
{
    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? roleId, [FromQuery] UserStatus? status, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUsersQuery(roleId, status), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(User.GetUserId()), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpPut("me")]
    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe(UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserProfileCommand(
                User.GetUserId(),
                request.Name,
                request.PaternalSurname,
                request.MaternalSurname,
                request.Phone,
                request.Email),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var customerRoleId = configuration.GetValue<Guid>("Roles:CustomerRoleId");
        if (customerRoleId == Guid.Empty)
        {
            return Problem(
                title: "Customer role is not configured.",
                detail: "Configure Roles:CustomerRoleId before allowing public registration.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var result = await sender.Send(
            new CreateUserCommand(
                customerRoleId,
                request.Name,
                request.PaternalSurname,
                request.MaternalSurname,
                request.Phone,
                request.Email,
                request.Password),
            cancellationToken);

        return this.ToCreatedResult(result);
    }

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPatch("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new BlockUserCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPatch("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnblockUserCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeUserRoleCommand(id, request.RoleId), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("me/refresh-tokens")]
    public async Task<IActionResult> GetMyRefreshTokens(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRefreshTokensByUserQuery(User.GetUserId()), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet("{id:guid}/refresh-tokens")]
    public async Task<IActionResult> GetUserRefreshTokens(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRefreshTokensByUserQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }
}

public sealed record RegisterUserRequest(string Name, string PaternalSurname, string? MaternalSurname, string Phone, string Email, string Password);

public sealed record UpdateMyProfileRequest(string Name, string PaternalSurname, string? MaternalSurname, string Phone, string Email);

public sealed record ChangeUserRoleRequest(Guid RoleId);
