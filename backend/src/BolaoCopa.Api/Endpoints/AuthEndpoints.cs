using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Authentication;
using BolaoCopa.Application.Authentication.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BolaoCopa.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        // POST /auth/register - cadastra um novo usuário e retorna seus dados com o token JWT.
        group.MapPost("/register", async (
            RegisterRequest request,
            AuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.RegisterAsync(request, cancellationToken);

            return result.Succeeded
                ? Results.Ok(result.Value)
                : MapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // POST /auth/login - autentica um usuário existente e retorna seus dados com o token JWT.
        group.MapPost("/login", async (
            LoginRequest request,
            AuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.LoginAsync(request, cancellationToken);

            return result.Succeeded
                ? Results.Ok(result.Value)
                : MapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // GET /auth/test - valida se o token JWT enviado pertence a um usuário autenticado.
        group.MapGet("/test", [Authorize] () => Results.Ok(new { message = "Authenticated." }));

        return app;
    }

    private static IResult MapFailure(HttpContext httpContext, AuthErrorCode errorCode, string? errorMessage)
    {
        var detail = errorMessage ?? "Authentication request failed.";

        return errorCode switch
        {
            AuthErrorCode.EmailAlreadyExists => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status409Conflict,
                "Email already registered.",
                detail),
            AuthErrorCode.InvalidCredentials => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status401Unauthorized,
                "Invalid credentials.",
                detail),
            AuthErrorCode.ValidationFailed => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                detail),
            _ => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Authentication request failed.",
                detail)
        };
    }
}
