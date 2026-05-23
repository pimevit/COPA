using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BolaoCopa.Application.Ranking;

namespace BolaoCopa.Api.Endpoints;

public static class RankingEndpoints
{
    public static IEndpointRouteBuilder MapRankingEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /ranking - retorna o ranking geral de usuários com pontuação e posição atual.
        app.MapGet("/ranking", async (
            RankingService rankingService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var ranking = await rankingService.GetRankingAsync(
                getOptionalCurrentUserId(httpContext),
                cancellationToken);

            return Results.Ok(ranking);
        });

        return app;
    }

    private static int? getOptionalCurrentUserId(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
