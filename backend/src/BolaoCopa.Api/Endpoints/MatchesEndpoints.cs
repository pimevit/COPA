using BolaoCopa.Api.Errors;
using BolaoCopa.Application.Matches;
using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BolaoCopa.Api.Endpoints;

public static class MatchesEndpoints
{
    public static IEndpointRouteBuilder MapMatchesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/matches");

        // GET /matches - lista as partidas, opcionalmente filtradas por fase e status.
        group.MapGet(string.Empty, async (
            string? stage,
            string? status,
            MatchesService matchesService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!tryCreateQuery(stage, status, httpContext, out var query, out var error))
            {
                return error;
            }

            var matches = await matchesService.ListAsync(query, cancellationToken);
            return Results.Ok(matches);
        });

        // GET /matches/{id} - busca os detalhes de uma partida específica pelo id.
        group.MapGet("/{id:int}", async (
            int id,
            MatchesService matchesService,
            CancellationToken cancellationToken) =>
        {
            var match = await matchesService.FindByIdAsync(id, cancellationToken);

            return match is null
                ? Results.NotFound()
                : Results.Ok(match);
        });

        // POST /matches - permite ao admin cadastrar uma nova partida com times existentes.
        group.MapPost(string.Empty, [Authorize(Policy = "AdminOnly")] async (
            CreateMatchRequest request,
            MatchAdminService matchAdminService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await matchAdminService.CreateAsync(request, cancellationToken);

            return result.Succeeded
                ? Results.Created($"/matches/{result.Value!.Id}", result.Value)
                : mapResultFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // DELETE /matches/{id} - permite ao admin excluir uma partida cadastrada.
        group.MapDelete("/{id:int}", [Authorize(Policy = "AdminOnly")] async (
            int id,
            bool? deleteBets,
            MatchAdminService matchAdminService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await matchAdminService.DeleteAsync(id, deleteBets == true, cancellationToken);

            return result.Succeeded
                ? Results.NoContent()
                : mapResultFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // PUT /matches/{id}/betting-lock - bloqueia ou desbloqueia manualmente os palpites de uma partida.
        group.MapPut("/{id:int}/betting-lock", [Authorize(Policy = "AdminOnly")] async (
            int id,
            UpdateMatchBettingLockRequest request,
            MatchAdminService matchAdminService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await matchAdminService.UpdateBettingLockAsync(id, request, cancellationToken);

            return result.Succeeded
                ? Results.NoContent()
                : mapResultFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        // PUT /matches/{id}/result - permite ao admin atualizar o resultado de uma partida.
        group.MapPut("/{id:int}/result", [Authorize(Policy = "AdminOnly")] async (
            int id,
            UpdateMatchResultRequest request,
            MatchResultsService matchResultsService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await matchResultsService.UpdateResultAsync(id, request, cancellationToken);

            return result.Succeeded
                ? Results.Ok(result.Value)
                : mapResultFailure(httpContext, result.ErrorCode, result.ErrorMessage);
        });

        return app;
    }

    private static bool tryCreateQuery(
        string? stage,
        string? status,
        HttpContext httpContext,
        out MatchesQuery query,
        out IResult error)
    {
        query = new MatchesQuery(null, null);
        error = Results.Empty;

        if (!tryParseFilter<Stage>(stage, "Stage", httpContext, out var parsedStage, out error))
        {
            return false;
        }

        if (!tryParseFilter<MatchStatus>(status, "Status", httpContext, out var parsedStatus, out error))
        {
            return false;
        }

        query = new MatchesQuery(parsedStage, parsedStatus);
        return true;
    }

    private static bool tryParseFilter<TEnum>(
        string? value,
        string filterName,
        HttpContext httpContext,
        out TEnum? parsed,
        out IResult error)
        where TEnum : struct, Enum
    {
        parsed = null;
        error = Results.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var result)
            && Enum.IsDefined(result))
        {
            parsed = result;
            return true;
        }

        var allowedValues = string.Join(", ", Enum.GetNames<TEnum>());
        error = ApiProblemDetailsFactory.CreateProblem(
            httpContext,
            StatusCodes.Status400BadRequest,
            $"Invalid {filterName} filter.",
            $"'{value}' is not a valid {filterName}. Allowed values: {allowedValues}.");

        return false;
    }

    private static IResult mapResultFailure(
        HttpContext httpContext,
        MatchResultErrorCode errorCode,
        string? errorMessage)
    {
        var detail = errorMessage ?? "Match result request failed.";

        return errorCode switch
        {
            MatchResultErrorCode.MatchNotFound => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status404NotFound,
                "Match not found.",
                detail),
            MatchResultErrorCode.TeamNotFound => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status404NotFound,
                "Team not found.",
                detail),
            MatchResultErrorCode.SameTeams => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Invalid teams.",
                detail),
            MatchResultErrorCode.DuplicateMatch => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status409Conflict,
                "Match already exists.",
                detail),
            MatchResultErrorCode.MatchHasBets => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status409Conflict,
                "Match has related bets.",
                detail),
            _ => ApiProblemDetailsFactory.CreateProblem(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Match result request failed.",
                detail)
        };
    }
}
