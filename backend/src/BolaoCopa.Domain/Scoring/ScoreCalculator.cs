using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Domain.Scoring;

public sealed class ScoreCalculator
{
    public int CalculateScore(
        int homeGoalsPrediction,
        int awayGoalsPrediction,
        int homeGoalsResult,
        int awayGoalsResult,
        Stage stage)
    {
        validateGoals(homeGoalsPrediction, nameof(homeGoalsPrediction));
        validateGoals(awayGoalsPrediction, nameof(awayGoalsPrediction));
        validateGoals(homeGoalsResult, nameof(homeGoalsResult));
        validateGoals(awayGoalsResult, nameof(awayGoalsResult));

        if (!Enum.IsDefined(stage))
        {
            throw new ArgumentOutOfRangeException(nameof(stage), stage, "Stage is not supported.");
        }

        var baseScore = calculateBaseScore(
            homeGoalsPrediction,
            awayGoalsPrediction,
            homeGoalsResult,
            awayGoalsResult);

        return baseScore * getMultiplier(stage);
    }

    private static int calculateBaseScore(
        int homeGoalsPrediction,
        int awayGoalsPrediction,
        int homeGoalsResult,
        int awayGoalsResult)
    {
        if (homeGoalsPrediction == homeGoalsResult && awayGoalsPrediction == awayGoalsResult)
        {
            return 5;
        }

        var matchedHomeGoals = homeGoalsPrediction == homeGoalsResult;
        var matchedAwayGoals = awayGoalsPrediction == awayGoalsResult;
        var matchedExactlyOneTeamGoals = matchedHomeGoals ^ matchedAwayGoals;
        var matchedOutcome = getOutcome(homeGoalsPrediction, awayGoalsPrediction) == getOutcome(homeGoalsResult, awayGoalsResult);

        if (matchedOutcome && matchedExactlyOneTeamGoals)
        {
            return 3;
        }

        if (matchedOutcome)
        {
            return 2;
        }

        return matchedExactlyOneTeamGoals ? 1 : 0;
    }

    private static MatchOutcome getOutcome(int homeGoals, int awayGoals)
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

    private static int getMultiplier(Stage stage)
    {
        return stage switch
        {
            Stage.Groups => 1,
            Stage.RoundOf16 => 2,
            Stage.QuarterFinals => 3,
            Stage.SemiFinals => 4,
            Stage.Final => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Stage is not supported.")
        };
    }

    private static void validateGoals(int goals, string parameterName)
    {
        if (goals < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, goals, "Goals cannot be negative.");
        }
    }

    private enum MatchOutcome
    {
        HomeWin,
        AwayWin,
        Draw
    }
}
