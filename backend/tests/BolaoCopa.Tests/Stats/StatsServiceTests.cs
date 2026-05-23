using BolaoCopa.Application.Stats;
using BolaoCopa.Application.Stats.Data;
using BolaoCopa.Application.Stats.ReadModels;
using Xunit;

namespace BolaoCopa.Tests.Stats;

public sealed class StatsServiceTests
{
    private static readonly DateTime BaseMatchDate = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetMyStatsAsync_CalculatesAllMetrics()
    {
        var service = createService(
            createBet(points: 5, homePrediction: 2, awayPrediction: 0, homeResult: 2, awayResult: 0, matchOffset: 1),
            createBet(points: 2, homePrediction: 1, awayPrediction: 0, homeResult: 2, awayResult: 0, matchOffset: 2),
            createBet(points: 0, homePrediction: 0, awayPrediction: 1, homeResult: 1, awayResult: 1, matchOffset: 3),
            createBet(points: 1, homePrediction: 3, awayPrediction: 1, homeResult: 3, awayResult: 0, matchOffset: 4));

        var stats = await service.GetMyStatsAsync(userId: 10);

        Assert.Equal(8, stats.TotalPoints);
        Assert.Equal(1, stats.ExactScoreCount);
        Assert.Equal(3, stats.WinnerHitCount);
        Assert.Equal(0.75, stats.HitPercentage);
        Assert.Equal(2, stats.BestHitStreak);
        Assert.Equal("/bets/me", stats.HistoryEndpoint);
    }

    [Fact]
    public async Task GetMyStatsAsync_WhenUserHasNoEvaluatedBets_ReturnsZeroMetrics()
    {
        var service = createService();

        var stats = await service.GetMyStatsAsync(userId: 10);

        Assert.Equal(0, stats.TotalPoints);
        Assert.Equal(0, stats.ExactScoreCount);
        Assert.Equal(0, stats.WinnerHitCount);
        Assert.Equal(0, stats.HitPercentage);
        Assert.Equal(0, stats.BestHitStreak);
    }

    [Fact]
    public async Task GetMyStatsAsync_UsesRepositoryUserFilter()
    {
        var service = new StatsService(new FakeStatsReadRepository(
            new Dictionary<int, IReadOnlyList<StatsBetReadModel>>
            {
                [10] = [createBet(points: 5)],
                [20] = [createBet(points: 5), createBet(points: 2)]
            }));

        var stats = await service.GetMyStatsAsync(userId: 10);

        Assert.Equal(5, stats.TotalPoints);
        Assert.Equal(1, stats.ExactScoreCount);
    }

    [Fact]
    public async Task GetMyStatsAsync_BestHitStreak_OrdersByMatchDateThenMatchId()
    {
        var service = createService(
            createBet(matchId: 3, points: 0, matchOffset: 1),
            createBet(matchId: 2, points: 2, matchOffset: 1),
            createBet(matchId: 1, points: 5, matchOffset: 0),
            createBet(matchId: 4, points: 1, matchOffset: 2));

        var stats = await service.GetMyStatsAsync(userId: 10);

        Assert.Equal(2, stats.BestHitStreak);
    }

    private static StatsService createService(params StatsBetReadModel[] bets)
    {
        return new StatsService(new FakeStatsReadRepository(
            new Dictionary<int, IReadOnlyList<StatsBetReadModel>>
            {
                [10] = bets
            }));
    }

    private static StatsBetReadModel createBet(
        int points,
        int matchId = 1,
        int homePrediction = 2,
        int awayPrediction = 0,
        int homeResult = 2,
        int awayResult = 0,
        int matchOffset = 0)
    {
        return new StatsBetReadModel(
            matchId,
            points,
            homePrediction,
            awayPrediction,
            homeResult,
            awayResult,
            BaseMatchDate.AddDays(matchOffset));
    }

    private sealed class FakeStatsReadRepository(IReadOnlyDictionary<int, IReadOnlyList<StatsBetReadModel>> betsByUser)
        : IStatsReadRepository
    {
        public Task<IReadOnlyList<StatsBetReadModel>> ListEvaluatedBetsByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                betsByUser.TryGetValue(userId, out var bets)
                    ? bets
                    : []);
        }
    }
}
