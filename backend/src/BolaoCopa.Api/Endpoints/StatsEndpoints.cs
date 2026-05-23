using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Stats;
using Microsoft.AspNetCore.Authorization;

namespace BolaoCopa.Api.Endpoints;

public static class StatsEndpoints
{
    public static IEndpointRouteBuilder MapStatsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /stats/me - retorna as estatísticas de desempenho do usuário autenticado.
        app.MapGet("/stats/me", [Authorize] async (
            StatsService statsService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!tryGetUserId(httpContext, out var userId, out var error))
            {
                return error;
            }

            var stats = await statsService.GetMyStatsAsync(userId, cancellationToken);
            return Results.Ok(stats);
        });

        return app;
    }

    private static bool tryGetUserId(HttpContext httpContext, out int userId, out IResult error)
    {
        userId = 0;
        error = Results.Empty;

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (int.TryParse(userIdClaim, out userId))
        {
            return true;
        }

        error = ApiProblemDetailsFactory.CreateProblem(
            httpContext,
            StatusCodes.Status401Unauthorized,
            "Invalid authenticated user.",
            "The authenticated token does not contain a valid user id.");

        return false;
    }
}
