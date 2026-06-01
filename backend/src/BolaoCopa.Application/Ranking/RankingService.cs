using BolaoCopa.Application.Common.Scoring;
using BolaoCopa.Application.Ranking.Contracts;
using BolaoCopa.Application.Ranking.Data;
using BolaoCopa.Application.Ranking.ReadModels;

namespace BolaoCopa.Application.Ranking;

public sealed class RankingService(IRankingReadRepository rankingReadRepository)
{
    public async Task<IReadOnlyList<RankingItemResponse>> GetRankingAsync(
        int? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        var bets = await rankingReadRepository.ListEvaluatedBetsAsync(cancellationToken);

        var rankedUsers = bets
            .GroupBy(bet => new { bet.UserId, bet.UserName })
            .Select(group => createRankingCandidate(group.Key.UserId, group.Key.UserName, group))
            .OrderByDescending(candidate => candidate.TotalPoints)
            .ThenByDescending(candidate => candidate.ExactScores)
            .ThenByDescending(candidate => candidate.OutcomeHits)
            .ThenByDescending(candidate => candidate.BestHitStreak)
            .ThenBy(candidate => candidate.FirstBetCreatedAt)
            .ThenBy(candidate => candidate.UserId)
            .ToList();

        return rankedUsers
            .Select((candidate, index) =>
            {
                var position = index + 1;

                return new RankingItemResponse(
                    position,
                    candidate.UserId,
                    candidate.UserName,
                    candidate.TotalPoints,
                    position <= 3,
                    currentUserId.HasValue && candidate.UserId == currentUserId.Value,
                    new RankingTieBreakersResponse(
                        candidate.ExactScores,
                        candidate.OutcomeHits,
                        candidate.BestHitStreak,
                        ensureUtc(candidate.FirstBetCreatedAt)));
            })
            .ToList();
    }

    private static RankingCandidate createRankingCandidate(
        int userId,
        string userName,
        IEnumerable<RankingBetReadModel> bets)
    {
        var evaluatedBets = bets.ToList();

        return new RankingCandidate(
            userId,
            userName,
            evaluatedBets.Sum(bet => bet.PointsEarned),
            evaluatedBets.Count(isExactScore),
            evaluatedBets.Count(isOutcomeHit),
            calculateBestHitStreak(evaluatedBets),
            evaluatedBets.Min(bet => bet.CreatedAt));
    }

    private static bool isExactScore(RankingBetReadModel bet)
    {
        return PredictionClassifier.IsExactScore(
            bet.HomeGoalsPrediction,
            bet.AwayGoalsPrediction,
            bet.HomeGoalsResult,
            bet.AwayGoalsResult);
    }

    private static bool isOutcomeHit(RankingBetReadModel bet)
    {
        return PredictionClassifier.IsOutcomeHit(
            bet.HomeGoalsPrediction,
            bet.AwayGoalsPrediction,
            bet.HomeGoalsResult,
            bet.AwayGoalsResult);
    }

    private static int calculateBestHitStreak(IEnumerable<RankingBetReadModel> bets)
    {
        var bestStreak = 0;
        var currentStreak = 0;

        foreach (var bet in bets.OrderBy(bet => bet.MatchDate).ThenBy(bet => bet.MatchId))
        {
            if (PredictionClassifier.IsHit(bet.PointsEarned))
            {
                currentStreak++;
                bestStreak = Math.Max(bestStreak, currentStreak);
                continue;
            }

            currentStreak = 0;
        }

        return bestStreak;
    }

    private static DateTime ensureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    private sealed record RankingCandidate(
        int UserId,
        string UserName,
        int TotalPoints,
        int ExactScores,
        int OutcomeHits,
        int BestHitStreak,
        DateTime FirstBetCreatedAt);

}
