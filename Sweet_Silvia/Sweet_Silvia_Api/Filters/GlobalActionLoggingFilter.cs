using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sweet_Silvia_Application.Abstractions.Audit;

namespace Sweet_Silvia_Api.Filters;

public sealed class GlobalActionLoggingFilter(
    ILogger<GlobalActionLoggingFilter> logger,
    IAuditLogWriter auditLogWriter)
    : IAsyncActionFilter
{
    private static readonly string[] SensitiveKeys =
    [
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization"
    ];

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogInformation("Executing action {ActionName}", context.ActionDescriptor.DisplayName);
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
                null,
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
        var forwardedFor = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static Dictionary<string, object?> SanitizeActionArguments(IDictionary<string, object?> actionArguments)
    {
        return actionArguments.ToDictionary(
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
}
