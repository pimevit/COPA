using BolaoCopa.Application.Ranking;
using BolaoCopa.Application.Ranking.Data;
using BolaoCopa.Application.Ranking.ReadModels;
using Xunit;

namespace BolaoCopa.Tests.Ranking;

public sealed class RankingServiceTests
{
    private static readonly DateTime BaseMatchDate = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime BaseCreatedAt = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetRankingAsync_OrdersByTotalPoints()
    {
        var service = createService(
            createBet(userId: 1, points: 2),
            createBet(userId: 2, points: 5));

        var ranking = await service.GetRankingAsync();

        Assert.Equal([2, 1], ranking.Select(item => item.UserId).ToArray());
        Assert.Equal([5, 2], ranking.Select(item => item.Points).ToArray());
        Assert.Equal([1, 2], ranking.Select(item => item.Position).ToArray());
    }

    [Fact]
    public async Task GetRankingAsync_WhenPointsTie_UsesExactScoresBeforeOutcomeHits()
    {
        var service = createService(
            createBet(userId: 1, points: 5, homePrediction: 2, awayPrediction: 0, homeResult: 2, awayResult: 0),
            createBet(userId: 2, points: 5, homePrediction: 1, awayPrediction: 0, homeResult: 2, awayResult: 0));

        var ranking = await service.GetRankingAsync();

        Assert.Equal([1, 2], ranking.Select(item => item.UserId).ToArray());
    }

    [Fact]
    public async Task GetRankingAsync_WhenExactScoresTie_UsesOutcomeHits()
    {
        var service = createService(
            createBet(userId: 1, points: 2, homePrediction: 0, awayPrediction: 1, homeResult: 2, awayResult: 0),
            createBet(userId: 2, points: 2, homePrediction: 1, awayPrediction: 0, homeResult: 2, awayResult: 0));

        var ranking = await service.GetRankingAsync();

        Assert.Equal([2, 1], ranking.Select(item => item.UserId).ToArray());
    }

    [Fact]
    public async Task GetRankingAsync_WhenOutcomeHitsTie_UsesBestHitStreak()
    {
        var service = createService(
            createBet(userId: 1, points: 2, matchOffset: 1),
            createBet(userId: 1, points: 0, matchOffset: 2, homePrediction: 0, awayPrediction: 1),
            createBet(userId: 1, points: 2, matchOffset: 3),
            createBet(userId: 2, points: 2, matchOffset: 1),
            createBet(userId: 2, points: 2, matchOffset: 2),
            createBet(userId: 2, points: 0, matchOffset: 3, homePrediction: 0, awayPrediction: 1));

        var ranking = await service.GetRankingAsync();

        Assert.Equal([2, 1], ranking.Select(item => item.UserId).ToArray());
    }

    [Fact]
    public async Task GetRankingAsync_WhenAllOtherCriteriaTie_UsesFirstBetCreatedAt()
    {
        var service = createService(
            createBet(userId: 1, points: 2, createdOffset: 2),
            createBet(userId: 2, points: 2, createdOffset: 1));

        var ranking = await service.GetRankingAsync();

        Assert.Equal([2, 1], ranking.Select(item => item.UserId).ToArray());
    }

    [Fact]
    public async Task GetRankingAsync_SetsTop3AndCurrentUserFlags()
    {
        var service = createService(
            createBet(userId: 1, points: 5),
            createBet(userId: 2, points: 4),
            createBet(userId: 3, points: 3),
            createBet(userId: 4, points: 2));

        var ranking = await service.GetRankingAsync(currentUserId: 4);

        Assert.Equal([true, true, true, false], ranking.Select(item => item.IsTop3).ToArray());
        Assert.Equal([false, false, false, true], ranking.Select(item => item.IsCurrentUser).ToArray());
    }

    private static RankingService createService(params RankingBetReadModel[] bets)
    {
        return new RankingService(new FakeRankingReadRepository(bets));
    }

    private static RankingBetReadModel createBet(
        int userId,
        int points,
        int homePrediction = 2,
        int awayPrediction = 0,
        int homeResult = 2,
        int awayResult = 0,
        int matchOffset = 0,
        int createdOffset = 0)
    {
        return new RankingBetReadModel(
            userId,
            $"User {userId}",
            matchOffset + 1,
            points,
            homePrediction,
            awayPrediction,
            homeResult,
            awayResult,
            BaseMatchDate.AddDays(matchOffset),
            BaseCreatedAt.AddMinutes(createdOffset));
    }

    private sealed class FakeRankingReadRepository(IReadOnlyList<RankingBetReadModel> bets)
        : IRankingReadRepository
    {
        public Task<IReadOnlyList<RankingBetReadModel>> ListEvaluatedBetsAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(bets);
        }
    }
}
