using System.Net;
using System.Text.Json;
using AssignmentSystem.Api.Common;

namespace AssignmentSystem.Api.Middleware;

/// <summary>
/// Sits at the very top of the pipeline (registered first in Program.cs) so it
/// wraps every downstream failure - controller, validator, service, or EF Core.
///
/// Design: services throw typed AppExceptions (see Common/AppExceptions.cs) for
/// expected business-rule failures. This middleware is the ONLY place that maps
/// those to HTTP status codes, so controllers stay free of try/catch blocks and
/// status-code decisions live in exactly one place.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode) = exception switch
        {
            NotFoundAppException => (HttpStatusCode.NotFound, "NotFound"),
            UnauthorizedAppException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ForbiddenAppException => (HttpStatusCode.Forbidden, "Forbidden"),
            ValidationAppException => (HttpStatusCode.BadRequest, "ValidationError"),
            ConflictAppException => (HttpStatusCode.Conflict, "Conflict"),
            _ => (HttpStatusCode.InternalServerError, "InternalServerError")
        };

        // Unexpected exceptions get logged with full detail server-side...
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            // Expected business-rule failures are logged at a lower level - they are
            // not bugs, but authorization/validation failures are still worth tracking
            // (see logging requirements: "authorization failures" must be logged).
            _logger.LogWarning("{ErrorCode} for {Method} {Path}: {Message}",
                errorCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        // ...but the client only ever gets a safe, generic message for 500s.
        // Exposing exception.Message or a stack trace to the client for
        // InternalServerError would leak internal implementation details.
        var clientMessage = statusCode == HttpStatusCode.InternalServerError
            ? "An unexpected error occurred. Please try again later."
            : exception.Message;

        var response = new ApiErrorResponse
        {
            Error = errorCode,
            Message = clientMessage,
            TraceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
