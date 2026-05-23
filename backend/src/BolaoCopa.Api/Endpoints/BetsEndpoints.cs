using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Bets;
using BolaoCopa.Application.Bets.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BolaoCopa.Api.Endpoints;

public static class BetsEndpoints
{
    public static IEndpointRouteBuilder MapBetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bets")
            .RequireAuthorization();

        // POST /bets - cria um palpite do usuário autenticado para uma partida.
        group.MapPost(string.Empty, [Authorize] async (
            CreateBetRequest request,
            BetsService betsService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!tryGetUserId(httpContext, out var userId, out var error))
            {
                return error;
            }

            var result = await betsService.CreateAsync(userId, request, cancellationToken);

            return result.Succeeded
                ? Results.Created($"/bets/{result.Value!.Id}", result.Value)
                : mapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // PUT /bets/{id} - atualiza um palpite existente do usuário autenticado.
        group.MapPut("/{id:int}", [Authorize] async (
            int id,
            UpdateBetRequest request,
            BetsService betsService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!tryGetUserId(httpContext, out var userId, out var error))
            {
                return error;
            }

            var result = await betsService.UpdateAsync(userId, id, request, cancellationToken);

            return result.Succeeded
                ? Results.Ok(result.Value)
                : mapFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // GET /bets/me - lista todos os palpites do usuário autenticado.
        group.MapGet("/me", [Authorize] async (
            BetsService betsService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!tryGetUserId(httpContext, out var userId, out var error))
            {
                return error;
            }

            var bets = await betsService.ListMineAsync(userId, cancellationToken);
            return Results.Ok(bets);
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

    private static IResult mapFailure(
        HttpContext httpContext,
        BetErrorCode errorCode,
        string? errorMessage)
    {
        var detail = errorMessage ?? "Bet request failed.";

        return errorCode switch
        {
            BetErrorCode.MatchNotFound => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status404NotFound,
                "Match not found.",
                detail),
            BetErrorCode.BetNotFound => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status404NotFound,
                "Bet not found.",
                detail),
            BetErrorCode.BettingWindowClosed => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status422UnprocessableEntity,
                "Betting window is closed.",
                detail),
            BetErrorCode.DuplicateBet => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status409Conflict,
                "Bet already exists.",
                detail),
            _ => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Bet request failed.",
                detail)
        };
    }
}
