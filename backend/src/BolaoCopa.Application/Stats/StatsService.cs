using BolaoCopa.Application.Common.Scoring;
using BolaoCopa.Application.Stats.Contracts;
using BolaoCopa.Application.Stats.Data;
using BolaoCopa.Application.Stats.ReadModels;

namespace BolaoCopa.Application.Stats;

public sealed class StatsService(IStatsReadRepository statsReadRepository)
{
    private const string HistoryEndpoint = "/bets/me";

    public async Task<MyStatsResponse> GetMyStatsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var bets = await statsReadRepository.ListEvaluatedBetsByUserAsync(userId, cancellationToken);
        var evaluatedCount = bets.Count;
        var hitCount = bets.Count(bet => PredictionClassifier.IsHit(bet.PointsEarned));

        return new MyStatsResponse(
            bets.Sum(bet => bet.PointsEarned),
            bets.Count(isExactScore),
            bets.Count(isWinnerHit),
            evaluatedCount == 0 ? 0 : (double)hitCount / evaluatedCount,
            calculateBestHitStreak(bets),
            HistoryEndpoint);
    }

    private static bool isExactScore(StatsBetReadModel bet)
    {
        return PredictionClassifier.IsExactScore(
            bet.HomeGoalsPrediction,
            bet.AwayGoalsPrediction,
            bet.HomeGoalsResult,
            bet.AwayGoalsResult);
    }

    private static bool isWinnerHit(StatsBetReadModel bet)
    {
        return PredictionClassifier.IsOutcomeHit(
            bet.HomeGoalsPrediction,
            bet.AwayGoalsPrediction,
            bet.HomeGoalsResult,
            bet.AwayGoalsResult);
    }

    private static int calculateBestHitStreak(IEnumerable<StatsBetReadModel> bets)
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
}
