using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Application.Matches;

public sealed class MatchResultsService(
    IMatchResultRepository matchResultRepository,
    MatchPointsRecalculator matchPointsRecalculator)
{
    public Task<MatchResult<MatchResultResponse>> UpdateResultAsync(
        int matchId,
        UpdateMatchResultRequest request,
        CancellationToken cancellationToken = default)
    {
        return matchResultRepository.ExecuteInTransactionAsync(async operationCancellationToken =>
        {
            var match = await matchResultRepository.FindMatchWithBetsAsync(matchId, operationCancellationToken);
            if (match is null)
            {
                return MatchResult<MatchResultResponse>.Failure(
                    MatchResultErrorCode.MatchNotFound,
                    "Match was not found.");
            }

            match.HomeGoals = request.HomeGoals!.Value;
            match.AwayGoals = request.AwayGoals!.Value;
            match.Status = MatchStatus.Finished;

            var recalculatedBets = matchPointsRecalculator.Recalculate(match);

            return MatchResult<MatchResultResponse>.Success(new MatchResultResponse(
                match.Id,
                match.HomeGoals.Value,
                match.AwayGoals.Value,
                match.Status.ToString(),
                recalculatedBets));
        }, cancellationToken);
    }
}
