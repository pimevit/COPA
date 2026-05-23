namespace BolaoCopa.Application.Stats.Contracts;

public sealed record MyStatsResponse(
    int TotalPoints,
    int ExactScoreCount,
    int WinnerHitCount,
    double HitPercentage,
    int BestHitStreak,
    string HistoryEndpoint);
