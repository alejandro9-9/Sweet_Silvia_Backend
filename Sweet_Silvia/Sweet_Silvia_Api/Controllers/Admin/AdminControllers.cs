using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.RefreshToken;
using Sweet_Silvia_Application.Role;

namespace Sweet_Silvia_Api.Controllers.Admin;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = ApiRoles.Administrator)]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetRolesQuery(onlyActive), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var id = request.Id.GetValueOrDefault(Guid.NewGuid());
        return this.ToCreatedResult(await sender.Send(new CreateRoleCommand(id, request.Name), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateRoleCommand(id, request.Name), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteRoleCommand(id), cancellationToken));
}

[ApiController]
[Route("api/refresh-tokens")]
[Authorize(Roles = ApiRoles.Administrator)]
public sealed class RefreshTokensController(ISender sender) : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetRefreshTokensByUserQuery(userId), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new RevokeRefreshTokenCommand(id), cancellationToken));
}

public sealed record CreateRoleRequest(Guid? Id, string Name);
public sealed record UpdateRoleRequest(string Name);
