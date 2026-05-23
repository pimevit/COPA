using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Matches;
using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Application.Matches.ReadModels;
using BolaoCopa.Domain.Enums;
using Xunit;

namespace BolaoCopa.Tests.Matches;

public sealed class MatchesServiceTests
{
    private static readonly DateTime FixedNowUtc = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ListAsync_ForwardsFiltersAndMapsResponses()
    {
        var repository = new FakeMatchReadRepository(
        [
            createMatch(1, Stage.Groups, MatchStatus.Scheduled, FixedNowUtc.AddMinutes(10))
        ]);
        var service = new MatchesService(repository, new FixedUtcClock(FixedNowUtc));
        var query = new MatchesQuery(Stage.Groups, MatchStatus.Scheduled);

        var matches = await service.ListAsync(query);
        var match = Assert.Single(matches);

        Assert.Equal(query, repository.LastQuery);
        Assert.Equal(1, match.Id);
        Assert.Equal("Brazil", match.HomeTeam.Name);
        Assert.Equal("Germany", match.AwayTeam.Name);
        Assert.Equal("Groups", match.Stage);
        Assert.Equal("Scheduled", match.Status);
        Assert.True(match.IsBettingOpen);
    }

    [Fact]
    public async Task ListAsync_CalculatesBettingOpenBeforeAtAndAfterLimit()
    {
        var repository = new FakeMatchReadRepository(
        [
            createMatch(1, Stage.Groups, MatchStatus.Scheduled, FixedNowUtc.AddSeconds(1)),
            createMatch(2, Stage.Groups, MatchStatus.Scheduled, FixedNowUtc),
            createMatch(3, Stage.Groups, MatchStatus.Scheduled, FixedNowUtc.AddTicks(-1))
        ]);
        var service = new MatchesService(repository, new FixedUtcClock(FixedNowUtc));

        var matches = await service.ListAsync(new MatchesQuery(null, null));

        Assert.True(matches[0].IsBettingOpen);
        Assert.False(matches[1].IsBettingOpen);
        Assert.False(matches[2].IsBettingOpen);
    }

    [Fact]
    public async Task FindByIdAsync_WhenMissing_ReturnsNull()
    {
        var service = new MatchesService(
            new FakeMatchReadRepository([]),
            new FixedUtcClock(FixedNowUtc));

        var match = await service.FindByIdAsync(99);

        Assert.Null(match);
    }

    private static MatchReadModel createMatch(
        int id,
        Stage stage,
        MatchStatus status,
        DateTime allowBetUntil)
    {
        return new MatchReadModel(
            id,
            new TeamReadModel(1, "Brazil", "BRA", "https://example.test/bra.svg"),
            new TeamReadModel(2, "Germany", "GER", "https://example.test/ger.svg"),
            new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc),
            stage,
            status,
            null,
            null,
            allowBetUntil);
    }

    private sealed class FakeMatchReadRepository(IReadOnlyList<MatchReadModel> matches) : IMatchReadRepository
    {
        public MatchesQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<MatchReadModel>> ListAsync(
            MatchesQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(matches);
        }

        public Task<MatchReadModel?> FindByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(matches.SingleOrDefault(match => match.Id == id));
        }
    }

    private sealed class FixedUtcClock(DateTime utcNow) : IUtcClock
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
