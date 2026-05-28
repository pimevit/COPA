namespace BolaoCopa.Application.Bets.Contracts;

public sealed record PublicBetResponse(
    int MatchId,
    int UserId,
    string UserName,
    int HomeGoalsPrediction,
    int AwayGoalsPrediction,
    int PointsEarned,
    DateTime CreatedAt,
    bool IsCurrentUser);
