namespace BolaoCopa.Application.Common.Scoring;

public static class PredictionClassifier
{
    public static bool IsExactScore(
        int homeGoalsPrediction,
        int awayGoalsPrediction,
        int homeGoalsResult,
        int awayGoalsResult)
    {
        return homeGoalsPrediction == homeGoalsResult
            && awayGoalsPrediction == awayGoalsResult;
    }

    public static bool IsOutcomeHit(
        int homeGoalsPrediction,
        int awayGoalsPrediction,
        int homeGoalsResult,
        int awayGoalsResult)
    {
        // Placar exato tambem conta como acerto de vencedor/empate.
        return GetOutcome(homeGoalsPrediction, awayGoalsPrediction)
            == GetOutcome(homeGoalsResult, awayGoalsResult);
    }

    public static bool IsHit(int pointsEarned)
    {
        // Decisao provisoria da duvida 3.2: acerto = PointsEarned > 0.
        return pointsEarned > 0;
    }

    private static MatchOutcome GetOutcome(int homeGoals, int awayGoals)
    {
        if (homeGoals > awayGoals)
        {
            return MatchOutcome.HomeWin;
        }

        if (awayGoals > homeGoals)
        {
            return MatchOutcome.AwayWin;
        }

        return MatchOutcome.Draw;
    }

    private enum MatchOutcome
    {
        HomeWin,
        AwayWin,
        Draw
    }
}
