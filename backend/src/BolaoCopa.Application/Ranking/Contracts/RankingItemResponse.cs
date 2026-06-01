namespace BolaoCopa.Application.Ranking.Contracts;

public sealed record RankingItemResponse(
    int Position,
    int UserId,
    string Name,
    int Points,
    bool IsTop3,
    bool IsCurrentUser,
    RankingTieBreakersResponse TieBreakers);

public sealed record RankingTieBreakersResponse(
    int ExactScores,
    int OutcomeHits,
    int BestHitStreak,
    DateTime FirstBetCreatedAtUtc);
