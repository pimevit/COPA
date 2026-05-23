using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Application.Matches.Contracts;

public sealed record CreateMatchRequest(
    int? HomeTeamId,
    int? AwayTeamId,
    DateTime? MatchDate,
    Stage? Stage);
