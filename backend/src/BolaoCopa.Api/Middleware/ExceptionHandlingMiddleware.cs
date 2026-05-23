using Microsoft.AspNetCore.Mvc;

namespace BolaoCopa.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (BadHttpRequestException exception) when (!httpContext.Response.HasStarted)
        {
            logger.LogWarning(
                exception,
                "Invalid request {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            await WriteProblemAsync(
                httpContext,
                exception.StatusCode,
                "Invalid request.",
                environment.IsDevelopment() ? exception.Message : null);
        }
        catch (Exception exception) when (!httpContext.Response.HasStarted)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string? detail)
    {
        httpContext.Response.Clear();
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: httpContext.RequestAborted);
    }
}
