using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ResourcePlanningAccelist.WebAPI.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred while processing request {Path}", httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}