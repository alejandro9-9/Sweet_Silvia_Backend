using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sweet_Silvia_Api.Filters;

public sealed class GlobalExceptionsFilter(ILogger<GlobalExceptionsFilter> logger)
    : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        logger.LogError(context.Exception, "Request failed with status {StatusCode}", status);
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : "The request could not be completed.",
            Detail = context.Exception.Message
        })
        {
            StatusCode = status
        };
        context.ExceptionHandled = true;
    }
}
