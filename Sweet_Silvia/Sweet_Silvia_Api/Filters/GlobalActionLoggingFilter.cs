using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sweet_Silvia_Application.Abstractions.Audit;
using Sweet_Silvia_Infrastructure;

namespace Sweet_Silvia_Api.Filters;

public sealed class GlobalActionLoggingFilter(
    ILogger<GlobalActionLoggingFilter> logger,
    IAuditLogWriter auditLogWriter,
    ApplicationDbContext dbContext)
    : IAsyncActionFilter
{
    private static readonly string[] SensitiveKeys =
    [
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "answer",
        "signature"
    ];

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogInformation("Executing action {ActionName}", context.ActionDescriptor.DisplayName);
        Dictionary<string, object?>? oldValues = null;
        try
        {
            oldValues = await CaptureOldValuesAsync(context);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not capture previous values for action {ActionName}", context.ActionDescriptor.DisplayName);
        }

        var result = await next();
        logger.LogInformation(
            "Executed action {ActionName} with status {StatusCode}",
            context.ActionDescriptor.DisplayName,
            result.HttpContext.Response.StatusCode);

        if (!ShouldAudit(context, result))
        {
            return;
        }

        try
        {
            await auditLogWriter.WriteAsync(
                GetUserId(context),
                context.HttpContext.User.FindFirstValue(ClaimTypes.Role),
                $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}",
                GetEntityName(context),
                GetEntityId(context),
                GetIpAddress(context),
                context.HttpContext.Request.Headers.UserAgent.ToString(),
                oldValues,
                SanitizeActionArguments(context.ActionArguments),
                context.ActionDescriptor.DisplayName,
                context.HttpContext.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not write audit log for action {ActionName}", context.ActionDescriptor.DisplayName);
        }
    }

    private static bool ShouldAudit(ActionExecutingContext context, ActionExecutedContext result)
    {
        if (result.Exception is not null)
        {
            return false;
        }

        if (result.HttpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return false;
        }

        if (IsAuthEndpoint(context))
        {
            return false;
        }

        return context.HttpContext.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE";
    }

    private static bool IsAuthEndpoint(ActionExecutingContext context)
    {
        return context.HttpContext.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, object?>?> CaptureOldValuesAsync(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Method is not ("PUT" or "PATCH" or "DELETE"))
        {
            return null;
        }

        var entityId = GetEntityId(context);
        if (entityId is null)
        {
            return null;
        }

        var entityType = FindEntityType(dbContext, GetEntityName(context));
        if (entityType is null)
        {
            return null;
        }

        var key = entityType.FindPrimaryKey();
        if (key?.Properties.Count != 1)
        {
            return null;
        }

        var tableName = entityType.GetTableName();
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return null;
        }

        var storeObject = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
        var keyColumn = key.Properties[0].GetColumnName(storeObject);
        if (string.IsNullOrWhiteSpace(keyColumn))
        {
            return null;
        }

        var properties = entityType.GetProperties()
            .Where(property => !property.IsShadowProperty())
            .Select(property => new
            {
                property.Name,
                ColumnName = property.GetColumnName(storeObject)
            })
            .Where(property => !string.IsNullOrWhiteSpace(property.ColumnName))
            .ToList();

        if (properties.Count == 0)
        {
            return null;
        }

        var quotedColumns = string.Join(", ", properties.Select(property => QuoteIdentifier(property.ColumnName!)));
        var schemaPrefix = string.IsNullOrWhiteSpace(entityType.GetSchema()) ? string.Empty : $"{QuoteIdentifier(entityType.GetSchema()!)}.";

        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT {quotedColumns} FROM {schemaPrefix}{QuoteIdentifier(tableName)} WHERE {QuoteIdentifier(keyColumn)} = @id LIMIT 1";

        var idParameter = command.CreateParameter();
        idParameter.ParameterName = "@id";
        idParameter.Value = entityId.Value;
        command.Parameters.Add(idParameter);

        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            await dbContext.Database.OpenConnectionAsync(context.HttpContext.RequestAborted);
        }

        await using var reader = await command.ExecuteReaderAsync(context.HttpContext.RequestAborted);
        if (!await reader.ReadAsync(context.HttpContext.RequestAborted))
        {
            return null;
        }

        var values = new Dictionary<string, object?>();
        foreach (var property in properties)
        {
            var value = reader[property.ColumnName!];
            values[property.Name] = value is DBNull ? null : value;
        }

        return SanitizeDictionary(values);
    }

    private static IEntityType? FindEntityType(ApplicationDbContext dbContext, string controllerName)
    {
        var candidates = GetEntityNameCandidates(controllerName);
        return dbContext.Model.GetEntityTypes()
            .FirstOrDefault(entityType => candidates.Contains(entityType.ClrType.Name, StringComparer.OrdinalIgnoreCase));
    }

    private static HashSet<string> GetEntityNameCandidates(string controllerName)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            controllerName
        };

        if (controllerName.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add($"{controllerName[..^3]}y");
        }

        if (controllerName.EndsWith("es", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add(controllerName[..^2]);
        }

        if (controllerName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add(controllerName[..^1]);
        }

        return candidates;
    }

    private static Guid? GetUserId(ActionExecutingContext context)
    {
        var value = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static string GetEntityName(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        return string.IsNullOrWhiteSpace(controller) ? "Unknown" : controller;
    }

    private static Guid? GetEntityId(ActionExecutingContext context)
    {
        var routeId = context.RouteData.Values["id"]?.ToString();
        return Guid.TryParse(routeId, out var entityId) ? entityId : null;
    }

    private static string? GetIpAddress(ActionExecutingContext context)
    {
        return context.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static Dictionary<string, object?> SanitizeActionArguments(IDictionary<string, object?> actionArguments)
    {
        return actionArguments.ToDictionary(
            pair => pair.Key,
            pair => IsSensitive(pair.Key) ? "***" : SanitizeValue(pair.Value));
    }

    private static Dictionary<string, object?> SanitizeDictionary(Dictionary<string, object?> values)
    {
        return values.ToDictionary(
            pair => pair.Key,
            pair => IsSensitive(pair.Key) ? "***" : SanitizeValue(pair.Value));
    }

    private static object? SanitizeValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var json = JsonSerializer.SerializeToElement(value);
        return SanitizeJsonElement(json);
    }

    private static object? SanitizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(property => property.Name, property => IsSensitive(property.Name) ? "***" : SanitizeJsonElement(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(SanitizeJsonElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool IsSensitive(string key)
    {
        return SensitiveKeys.Any(sensitiveKey => key.Contains(sensitiveKey, StringComparison.OrdinalIgnoreCase));
    }

    private static string QuoteIdentifier(string value)
    {
        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
