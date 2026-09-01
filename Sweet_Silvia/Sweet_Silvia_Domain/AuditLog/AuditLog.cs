using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.AuditLog;

public sealed class AuditLog : Entity
{
    private AuditLog() { }

    private AuditLog(
        Guid id,
        Guid? actorUserId,
        string? actorRole,
        string action,
        string entityName,
        Guid? entityId,
        DateTimeOffset occurredAt,
        string? ipAddress,
        string? userAgent,
        string? oldValuesJson,
        string? newValuesJson,
        string? observation)
        : base(id)
    {
        ActorUserId = actorUserId;
        ActorRole = Normalize(actorRole, 80);
        Action = NormalizeRequired(action, nameof(action), 120);
        EntityName = NormalizeRequired(entityName, nameof(entityName), 120);
        EntityId = entityId;
        OccurredAt = occurredAt;
        IpAddress = Normalize(ipAddress, 64);
        UserAgent = Normalize(userAgent, 500);
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
        Observation = Normalize(observation, 500);
    }

    public Guid? ActorUserId { get; private set; }
    public string? ActorRole { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? OldValuesJson { get; private set; }
    public string? NewValuesJson { get; private set; }
    public string? Observation { get; private set; }

    public static AuditLog Create(
        Guid? actorUserId,
        string? actorRole,
        string action,
        string entityName,
        Guid? entityId,
        DateTimeOffset occurredAt,
        string? ipAddress,
        string? userAgent,
        string? oldValuesJson,
        string? newValuesJson,
        string? observation)
    {
        return new AuditLog(
            Guid.NewGuid(),
            actorUserId,
            actorRole,
            action,
            entityName,
            entityId,
            occurredAt,
            ipAddress,
            userAgent,
            oldValuesJson,
            newValuesJson,
            observation);
    }

    private static string NormalizeRequired(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The value is required.", parameterName);
        }

        return Normalize(value, maxLength)!;
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized[..Math.Min(normalized.Length, maxLength)];
    }
}
