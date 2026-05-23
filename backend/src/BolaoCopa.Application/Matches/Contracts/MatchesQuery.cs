using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Application.Matches.Contracts;

public sealed record MatchesQuery(Stage? Stage, MatchStatus? Status);
