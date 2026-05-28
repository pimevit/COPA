using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Admin.Users;

namespace BolaoCopa.Api.Endpoints;

public static class AdminUsersEndpoints
{
    public static IEndpointRouteBuilder MapAdminUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/users")
            .RequireAuthorization("AdminOnly");

        // GET /admin/users - lista usuários para manutenção administrativa.
        group.MapGet(string.Empty, async (
            AdminUsersService adminUsersService,
            CancellationToken cancellationToken) =>
        {
            var users = await adminUsersService.ListAsync(cancellationToken);
            return Results.Ok(users);
        });

        // POST /admin/users/{id}/reset-password - gera uma senha temporária para o usuário.
        group.MapPost("/{id:int}/reset-password", async (
            int id,
            AdminUsersService adminUsersService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await adminUsersService.ResetPasswordAsync(id, cancellationToken);

            return result is null
                ? ApiProblemDetailsFactory.CreateProblem(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "User not found.",
                    "User was not found.")
                : Results.Ok(result);
        });

        return app;
    }
}
