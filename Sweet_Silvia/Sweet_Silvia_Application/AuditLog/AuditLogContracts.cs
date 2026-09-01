using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.AuditLog;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? ActorUserId,
    string? ActorRole,
    string Action,
    string EntityName,
    Guid? EntityId,
    DateTimeOffset OccurredAt,
    string? IpAddress,
    string? UserAgent,
    string? OldValuesJson,
    string? NewValuesJson,
    string? Observation);

public sealed record GetAuditLogsQuery(
    Guid? ActorUserId,
    string? EntityName,
    Guid? EntityId,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To) : IQuery<List<AuditLogResponse>>;
