using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.AuditLog;

namespace Sweet_Silvia_Api.Controllers.AuditLogs;

[ApiController]
[Route("api/audit-logs")]
public sealed class AuditLogsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? actorUserId,
        [FromQuery] string? entityName,
        [FromQuery] Guid? entityId,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAuditLogsQuery(actorUserId, entityName, entityId, action, from, to),
            cancellationToken);

        return this.ToActionResult(result);
    }
}
