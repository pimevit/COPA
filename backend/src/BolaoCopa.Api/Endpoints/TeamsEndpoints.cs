using BolaoCopa.Application.Teams;
using Microsoft.AspNetCore.Authorization;

namespace BolaoCopa.Api.Endpoints;

public static class TeamsEndpoints
{
    public static IEndpointRouteBuilder MapTeamsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /teams - lista os times existentes para uso no painel administrativo.
        app.MapGet("/teams", [Authorize(Policy = "AdminOnly")] async (
            TeamsService teamsService,
            CancellationToken cancellationToken) =>
        {
            var teams = await teamsService.ListAsync(cancellationToken);
            return Results.Ok(teams);
        });

        return app;
    }
}
