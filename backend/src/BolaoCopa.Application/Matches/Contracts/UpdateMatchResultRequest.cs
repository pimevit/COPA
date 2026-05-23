namespace BolaoCopa.Application.Matches.Contracts;

public sealed record UpdateMatchResultRequest(
    int? HomeGoals,
    int? AwayGoals);
