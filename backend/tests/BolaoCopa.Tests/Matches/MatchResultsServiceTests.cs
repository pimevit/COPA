using BolaoCopa.Application.Matches;
using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using BolaoCopa.Domain.Scoring;
using Xunit;

namespace BolaoCopa.Tests.Matches;

public sealed class MatchResultsServiceTests
{
    [Fact]
    public async Task UpdateResultAsync_RecalculatesAllBetsForMatch()
    {
        var match = createMatch();
        match.Bets.Add(createBet(id: 1, matchId: 1, homePrediction: 2, awayPrediction: 1, pointsEarned: 99));
        match.Bets.Add(createBet(id: 2, matchId: 1, homePrediction: 1, awayPrediction: 0, pointsEarned: 99));
        var service = createService(new FakeMatchResultRepository(match));

        var result = await service.UpdateResultAsync(1, new UpdateMatchResultRequest(2, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(MatchStatus.Finished, match.Status);
        Assert.Equal(2, result.Value!.RecalculatedBets);
        Assert.Equal(5, match.Bets.Single(bet => bet.Id == 1).PointsEarned);
        Assert.Equal(2, match.Bets.Single(bet => bet.Id == 2).PointsEarned);
    }

    [Fact]
    public async Task UpdateResultAsync_WhenResultIsCorrected_ReplacesPreviousPoints()
    {
        var match = createMatch();
        match.Bets.Add(createBet(id: 1, matchId: 1, homePrediction: 2, awayPrediction: 1, pointsEarned: 0));
        var service = createService(new FakeMatchResultRepository(match));

        await service.UpdateResultAsync(1, new UpdateMatchResultRequest(2, 1));
        await service.UpdateResultAsync(1, new UpdateMatchResultRequest(2, 0));

        Assert.Equal(3, match.Bets.Single().PointsEarned);
    }

    [Fact]
    public async Task UpdateResultAsync_WhenSameResultIsSubmittedAgain_IsIdempotent()
    {
        var match = createMatch();
        match.Bets.Add(createBet(id: 1, matchId: 1, homePrediction: 2, awayPrediction: 1, pointsEarned: 0));
        var service = createService(new FakeMatchResultRepository(match));

        await service.UpdateResultAsync(1, new UpdateMatchResultRequest(2, 1));
        await service.UpdateResultAsync(1, new UpdateMatchResultRequest(2, 1));

        Assert.Single(match.Bets);
        Assert.Equal(5, match.Bets.Single().PointsEarned);
    }

    [Fact]
    public async Task UpdateResultAsync_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        var service = createService(new FakeMatchResultRepository());

        var result = await service.UpdateResultAsync(999, new UpdateMatchResultRequest(2, 1));

        Assert.False(result.Succeeded);
        Assert.Equal(MatchResultErrorCode.MatchNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateResultAsync_WhenRecalculationFails_RollsBackChanges()
    {
        var match = createMatch();
        match.Stage = (Stage)999;
        match.Bets.Add(createBet(id: 1, matchId: 1, homePrediction: 2, awayPrediction: 1, pointsEarned: 7));
        var service = createService(new FakeMatchResultRepository(match));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.UpdateResultAsync(1, new UpdateMatchResultRequest(2, 1)));

        Assert.Null(match.HomeGoals);
        Assert.Null(match.AwayGoals);
        Assert.Equal(MatchStatus.Scheduled, match.Status);
        Assert.Equal(7, match.Bets.Single().PointsEarned);
    }

    private static MatchResultsService createService(FakeMatchResultRepository repository)
    {
        return new MatchResultsService(
            repository,
            new MatchPointsRecalculator(new ScoreCalculator()));
    }

    private static Match createMatch()
    {
        return new Match
        {
            Id = 1,
            HomeTeamId = 1,
            AwayTeamId = 2,
            MatchDate = new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc),
            Stage = Stage.Groups,
            Status = MatchStatus.Scheduled,
            AllowBetUntil = new DateTime(2026, 6, 12, 17, 45, 0, DateTimeKind.Utc)
        };
    }

    private static Bet createBet(
        int id,
        int matchId,
        int homePrediction,
        int awayPrediction,
        int pointsEarned)
    {
        return new Bet
        {
            Id = id,
            UserId = id,
            MatchId = matchId,
            HomeGoalsPrediction = homePrediction,
            AwayGoalsPrediction = awayPrediction,
            PointsEarned = pointsEarned,
            CreatedAt = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    private sealed class FakeMatchResultRepository(params Match[] matches) : IMatchResultRepository
    {
        private readonly List<Match> matches = [.. matches];

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            var snapshots = this.matches.Select(createSnapshot).ToList();

            try
            {
                return await operation(cancellationToken);
            }
            catch
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Restore();
                }

                throw;
            }
        }

        public Task<Match?> FindMatchWithBetsAsync(
            int matchId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(matches.SingleOrDefault(match => match.Id == matchId));
        }

        private static MatchSnapshot createSnapshot(Match match)
        {
            return new MatchSnapshot(
                match,
                match.HomeGoals,
                match.AwayGoals,
                match.Status,
                match.Bets.Select(bet => new BetSnapshot(bet, bet.PointsEarned)).ToList());
        }

        private sealed record MatchSnapshot(
            Match Match,
            int? HomeGoals,
            int? AwayGoals,
            MatchStatus Status,
            IReadOnlyList<BetSnapshot> Bets)
        {
            public void Restore()
            {
                Match.HomeGoals = HomeGoals;
                Match.AwayGoals = AwayGoals;
                Match.Status = Status;

                foreach (var bet in Bets)
                {
                    bet.Bet.PointsEarned = bet.PointsEarned;
                }
            }
        }

        private sealed record BetSnapshot(Bet Bet, int PointsEarned);
    }
}
