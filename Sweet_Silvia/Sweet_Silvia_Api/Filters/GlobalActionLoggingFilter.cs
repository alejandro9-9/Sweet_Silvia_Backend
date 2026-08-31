using Microsoft.AspNetCore.Mvc.Filters;

namespace Sweet_Silvia_Api.Filters;

public sealed class GlobalActionLoggingFilter(ILogger<GlobalActionLoggingFilter> logger)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogInformation("Executing action {ActionName}", context.ActionDescriptor.DisplayName);
        var result = await next();
        logger.LogInformation(
            "Executed action {ActionName} with status {StatusCode}",
            context.ActionDescriptor.DisplayName,
            result.HttpContext.Response.StatusCode);
    }
}
