using BolaoCopa.Infrastructure.Admin;

namespace BolaoCopa.Api.Endpoints;

public static class AdminMaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapAdminMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/maintenance")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/teams/brasileirao-serie-a-2026", async (
            AdminMaintenanceService adminMaintenanceService,
            CancellationToken cancellationToken) =>
        {
            var result = await adminMaintenanceService.ImportBrasileiraoSerieA2026TeamsAsync(cancellationToken);
            return Results.Ok(result);
        });

        group.MapPost("/teams/world-cup-2026", async (
            AdminMaintenanceService adminMaintenanceService,
            CancellationToken cancellationToken) =>
        {
            var result = await adminMaintenanceService.ImportWorldCup2026TeamsAsync(cancellationToken);
            return Results.Ok(result);
        });

        group.MapDelete("/application-data", async (
            AdminMaintenanceService adminMaintenanceService,
            CancellationToken cancellationToken) =>
        {
            var result = await adminMaintenanceService.ClearApplicationDataAsync(cancellationToken);
            return Results.Ok(result);
        });

        group.MapPost("/recalculate-points", async (
            AdminMaintenanceService adminMaintenanceService,
            CancellationToken cancellationToken) =>
        {
            var result = await adminMaintenanceService.RecalculateFinishedMatchPointsAsync(cancellationToken);
            return Results.Ok(result);
        });

        return app;
    }
}
