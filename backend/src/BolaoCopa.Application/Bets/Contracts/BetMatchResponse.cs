using BolaoCopa.Application.Matches.Contracts;

namespace BolaoCopa.Application.Bets.Contracts;

public sealed record BetMatchResponse(
    int Id,
    TeamSummaryResponse HomeTeam,
    TeamSummaryResponse AwayTeam,
    DateTime MatchDate,
    string Stage,
    string Status,
    int? HomeGoals,
    int? AwayGoals);
