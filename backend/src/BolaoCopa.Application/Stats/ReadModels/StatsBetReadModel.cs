namespace BolaoCopa.Application.Stats.ReadModels;

public sealed record StatsBetReadModel(
    int MatchId,
    int PointsEarned,
    int HomeGoalsPrediction,
    int AwayGoalsPrediction,
    int HomeGoalsResult,
    int AwayGoalsResult,
    DateTime MatchDate);
