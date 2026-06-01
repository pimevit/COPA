using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Notices;
using BolaoCopa.Application.Notices.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BolaoCopa.Api.Endpoints;

public static class NoticesEndpoints
{
    public static IEndpointRouteBuilder MapNoticesEndpoints(this IEndpointRouteBuilder app)
    {
        var noticesGroup = app.MapGroup("/notices");

        // GET /notices/matches - retorna o recado exibido na tela de partidas.
        noticesGroup.MapGet("/matches", async (
            NoticesService noticesService,
            CancellationToken cancellationToken) =>
        {
            var notice = await noticesService.GetMatchesNoticeAsync(cancellationToken);
            return Results.Ok(notice);
        });

        var adminGroup = app.MapGroup("/admin/notices")
            .RequireAuthorization("AdminOnly");

        // PUT /admin/notices/matches - atualiza ou limpa o recado exibido na tela de partidas.
        adminGroup.MapPut("/matches", [Authorize(Policy = "AdminOnly")] async (
            UpdateNoticeRequest request,
            NoticesService noticesService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if ((request.Message?.Length ?? 0) > NoticesService.MaxMessageLength)
            {
                return ApiProblemDetailsFactory.CreateProblem(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Notice is too long.",
                    $"Message must contain at most {NoticesService.MaxMessageLength} characters.");
            }

            var notice = await noticesService.UpdateMatchesNoticeAsync(request.Message, cancellationToken);
            return Results.Ok(notice);
        });

        return app;
    }
}
