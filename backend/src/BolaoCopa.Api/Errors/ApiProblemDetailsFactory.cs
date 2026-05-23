using Microsoft.AspNetCore.Mvc;

namespace BolaoCopa.Api.Errors;

public static class ApiProblemDetailsFactory
{
    public static IResult CreateProblem(
        HttpContext httpContext,
        int statusCode,
        string title,
        string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return Results.Json(
            problemDetails,
            statusCode: statusCode,
            contentType: "application/problem+json");
    }

    public static IResult CreateValidationProblem(
        HttpContext httpContext,
        IDictionary<string, string[]> errors)
    {
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return Results.Json(
            problemDetails,
            statusCode: StatusCodes.Status400BadRequest,
            contentType: "application/problem+json");
    }
}
