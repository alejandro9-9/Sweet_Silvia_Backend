using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Application.Common;

namespace Sweet_Silvia_Api.Filters;

public sealed class GlobalExceptionsFilter(ILogger<GlobalExceptionsFilter> logger)
    : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var isOversizedRequest = context.Exception is BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge }
            || context.Exception is InvalidDataException invalidDataException
                && (invalidDataException.Message.Contains("request body too large", StringComparison.OrdinalIgnoreCase)
                    || invalidDataException.Message.Contains("multipart", StringComparison.OrdinalIgnoreCase)
                        && invalidDataException.Message.Contains("limit", StringComparison.OrdinalIgnoreCase));
        var status = isOversizedRequest ? StatusCodes.Status413PayloadTooLarge : context.Exception switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,
            TimeoutException or HttpRequestException => StatusCodes.Status503ServiceUnavailable,
            NotSupportedException => StatusCodes.Status415UnsupportedMediaType,
            InvalidDataException => StatusCodes.Status400BadRequest,
            InvalidOperationException invalidOperationException when UserFacingErrorMessages.IsExpectedConflict(invalidOperationException)
                => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        logger.LogError(context.Exception, "Request failed with status {StatusCode}", status);
        var problem = new ProblemDetails
        {
            Status = status,
            Title = UserFacingErrorMessages.GetTitle(status),
            Detail = UserFacingErrorMessages.ForException(context.Exception, status, isOversizedRequest)
        };
        problem.Extensions["code"] = GetErrorCode(context.Exception, status, isOversizedRequest);
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = status
        };
        context.ExceptionHandled = true;
    }

    private static string GetErrorCode(Exception exception, int statusCode, bool isOversizedRequest)
    {
        if (isOversizedRequest)
        {
            return "File.TooLarge";
        }

        return exception switch
        {
            BadHttpRequestException => "Request.InvalidData",
            UnauthorizedAccessException => "Auth.Unauthorized",
            KeyNotFoundException => "Resource.NotFound",
            DbUpdateConcurrencyException => "Data.ConcurrencyConflict",
            ArgumentException => "Request.InvalidData",
            TimeoutException => "Service.Timeout",
            HttpRequestException => "Service.ConnectionFailed",
            NotSupportedException => "File.UnsupportedFormat",
            InvalidDataException => "File.InvalidContent",
            InvalidOperationException when statusCode == StatusCodes.Status409Conflict => "Operation.NotAllowed",
            _ => "Server.Unexpected"
        };
    }
}
