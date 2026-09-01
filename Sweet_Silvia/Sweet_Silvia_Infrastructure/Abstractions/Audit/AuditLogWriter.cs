using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Audit;
using AuditLogEntity = Sweet_Silvia_Domain.AuditLog.AuditLog;

namespace Sweet_Silvia_Infrastructure.Abstractions.Audit;

internal sealed class AuditLogWriter(ApplicationDbContext context) : IAuditLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task WriteAsync(
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
        CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLogEntity.Create(
            actorUserId,
            actorRole,
            action,
            entityName,
            entityId,
            DateTimeOffset.UtcNow,
            ipAddress,
            userAgent,
            Serialize(oldValues),
            Serialize(newValues),
            observation);

        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string? Serialize(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }
}
