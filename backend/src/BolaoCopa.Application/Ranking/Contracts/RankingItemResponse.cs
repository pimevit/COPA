namespace BolaoCopa.Application.Ranking.Contracts;

public sealed record RankingItemResponse(
    int Position,
    int UserId,
    string Name,
    int Points,
    bool IsTop3,
    bool IsCurrentUser);
