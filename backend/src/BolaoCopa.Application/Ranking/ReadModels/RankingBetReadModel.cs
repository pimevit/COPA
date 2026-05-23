namespace BolaoCopa.Application.Ranking.ReadModels;

public sealed record RankingBetReadModel(
    int UserId,
    string UserName,
    int MatchId,
    int PointsEarned,
    int HomeGoalsPrediction,
    int AwayGoalsPrediction,
    int HomeGoalsResult,
    int AwayGoalsResult,
    DateTime MatchDate,
    DateTime CreatedAt);
