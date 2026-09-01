namespace Sweet_Silvia_Application.Abstractions.Audit;

public interface IAuditLogWriter
{
    Task WriteAsync(
        Guid? actorUserId,
        string? actorRole,
        string action,
        string entityName,
        Guid? entityId,
        string? ipAddress,
        string? userAgent,
        object? oldValues,
        object? newValues,
        string? observation,
        CancellationToken cancellationToken = default);
}
