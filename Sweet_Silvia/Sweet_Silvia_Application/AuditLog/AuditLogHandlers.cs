using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Abstractions;
using AuditLogEntity = Sweet_Silvia_Domain.AuditLog.AuditLog;

namespace Sweet_Silvia_Application.AuditLog;

internal sealed class GetAuditLogsQueryHandler(IRepository<AuditLogEntity> auditLogs)
    : IQueryHandler<GetAuditLogsQuery, List<AuditLogResponse>>
{
    public Task<Result<List<AuditLogResponse>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = auditLogs.Query();

        if (request.ActorUserId.HasValue)
        {
            query = query.Where(log => log.ActorUserId == request.ActorUserId);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            query = query.Where(log => log.EntityName == request.EntityName);
        }

        if (request.EntityId.HasValue)
        {
            query = query.Where(log => log.EntityId == request.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(log => log.Action == request.Action);
        }

        if (request.From.HasValue)
        {
            query = query.Where(log => log.OccurredAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(log => log.OccurredAt <= request.To.Value);
        }

        var response = query
            .OrderByDescending(log => log.OccurredAt)
            .Take(500)
            .Select(log => new AuditLogResponse(
                log.Id,
                log.ActorUserId,
                log.ActorRole,
                log.Action,
                log.EntityName,
                log.EntityId,
                log.OccurredAt,
                log.IpAddress,
                log.UserAgent,
                log.OldValuesJson,
                log.NewValuesJson,
                log.Observation))
            .ToList();

        return Task.FromResult(Result.Success(response));
    }
}
