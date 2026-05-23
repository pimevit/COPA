namespace BolaoCopa.Application.Bets.Contracts;

public sealed record BetResponse(
    int Id,
    int MatchId,
    int HomeGoalsPrediction,
    int AwayGoalsPrediction,
    int PointsEarned,
    DateTime CreatedAt,
    BetMatchResponse Match);
