namespace BolaoCopa.Application.Bets.Contracts;

public sealed record CreateBetRequest(
    int MatchId,
    int HomeGoalsPrediction,
    int AwayGoalsPrediction);
