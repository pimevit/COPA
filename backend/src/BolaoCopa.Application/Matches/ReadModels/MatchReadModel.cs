using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Application.Matches.ReadModels;

public sealed record MatchReadModel(
    int Id,
    TeamReadModel HomeTeam,
    TeamReadModel AwayTeam,
    DateTime MatchDate,
    Stage Stage,
    MatchStatus Status,
    int? HomeGoals,
    int? AwayGoals,
    DateTime AllowBetUntil,
    bool IsBettingLocked);
