namespace BolaoCopa.Application.Matches.Contracts;

public sealed record MatchDetailResponse(
    int Id,
    TeamSummaryResponse HomeTeam,
    TeamSummaryResponse AwayTeam,
    DateTime MatchDate,
    string Stage,
    string Status,
    int? HomeGoals,
    int? AwayGoals,
    bool IsBettingOpen,
    bool IsBettingLocked);
