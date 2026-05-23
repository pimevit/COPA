using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Scoring;

namespace BolaoCopa.Application.Matches;

public sealed class MatchPointsRecalculator(ScoreCalculator scoreCalculator)
{
    public int Recalculate(Match match)
    {
        if (match.HomeGoals is null || match.AwayGoals is null)
        {
            throw new InvalidOperationException("Match result must be complete before recalculating bets.");
        }

        foreach (var bet in match.Bets)
        {
            bet.PointsEarned = scoreCalculator.CalculateScore(
                bet.HomeGoalsPrediction,
                bet.AwayGoalsPrediction,
                match.HomeGoals.Value,
                match.AwayGoals.Value,
                match.Stage);
        }

        return match.Bets.Count;
    }
}
