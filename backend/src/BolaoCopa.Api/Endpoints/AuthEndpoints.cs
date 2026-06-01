using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Authentication;
using BolaoCopa.Application.Authentication.Contracts;
using BolaoCopa.Application.Authentication.RefreshTokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BolaoCopa.Api.Endpoints;

public static class AuthEndpoints
{
    private const string RefreshTokenCookieName = "bolao-copa-refresh-token";
    private const string RefreshTokenCookiePath = "/auth";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        // POST /auth/register - cadastra um novo usuário e retorna seus dados com o token JWT.
        group.MapPost("/register", async (
            RegisterRequest request,
            AuthService authService,
            RefreshTokenService refreshTokenService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.RegisterAsync(request, cancellationToken);

            if (!result.Succeeded)
            {
                return MapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
            }

            var refreshToken = await refreshTokenService.CreateForUserAsync(
                result.Value!.User.Id,
                cancellationToken);

            appendRefreshTokenCookie(httpContext, refreshToken);

            return Results.Ok(result.Value);
        });

        // POST /auth/login - autentica um usuário existente e retorna seus dados com o token JWT.
        group.MapPost("/login", async (
            LoginRequest request,
            AuthService authService,
            RefreshTokenService refreshTokenService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.LoginAsync(request, cancellationToken);

            if (!result.Succeeded)
            {
                return MapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
            }

            var refreshToken = await refreshTokenService.CreateForUserAsync(
                result.Value!.User.Id,
                cancellationToken);

            appendRefreshTokenCookie(httpContext, refreshToken);

            return Results.Ok(result.Value);
        });

        // POST /auth/refresh - renova o JWT usando o refresh token salvo em cookie HttpOnly.
        group.MapPost("/refresh", async (
            RefreshTokenService refreshTokenService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
            var result = await refreshTokenService.RefreshAsync(refreshToken, cancellationToken);

            if (!result.Succeeded)
            {
                clearRefreshTokenCookie(httpContext);
                return MapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
            }

            appendRefreshTokenCookie(httpContext, result.Value!.RefreshToken);

            return Results.Ok(result.Value.Session);
        });

        // POST /auth/logout - revoga a sessão atual baseada no refresh token do cookie.
        group.MapPost("/logout", async (
            RefreshTokenService refreshTokenService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
            await refreshTokenService.RevokeAsync(refreshToken, cancellationToken);
            clearRefreshTokenCookie(httpContext);

            return Results.NoContent();
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

    private static void appendRefreshTokenCookie(HttpContext httpContext, RefreshTokenIssue refreshToken)
    {
        httpContext.Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken.Token,
            createRefreshTokenCookieOptions(httpContext, refreshToken.ExpiresAtUtc));
    }

    private static void clearRefreshTokenCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(
            RefreshTokenCookieName,
            createRefreshTokenCookieOptions(httpContext, DateTime.UnixEpoch));
    }

    private static CookieOptions createRefreshTokenCookieOptions(HttpContext httpContext, DateTime expiresAtUtc)
    {
        var environment = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
        var isDevelopment = environment.IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = new DateTimeOffset(expiresAtUtc),
            Path = RefreshTokenCookiePath
        };
    }
}
