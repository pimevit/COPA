namespace BolaoCopa.Application.Matches.Contracts;

public sealed record MatchResultResponse(
    int Id,
    int HomeGoals,
    int AwayGoals,
    string Status,
    int RecalculatedBets);
