namespace BolaoCopa.Application.Bets.Contracts;

public sealed record UpdateBetRequest(
    int HomeGoalsPrediction,
    int AwayGoalsPrediction);
