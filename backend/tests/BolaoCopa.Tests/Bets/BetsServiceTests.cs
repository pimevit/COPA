using BolaoCopa.Application.Bets;
using BolaoCopa.Application.Bets.Contracts;
using BolaoCopa.Application.Bets.Data;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using Xunit;

namespace BolaoCopa.Tests.Bets;

public sealed class BetsServiceTests
{
    private static readonly DateTime FixedNowUtc = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_WhenWindowIsOpen_SavesBetWithZeroPoints()
    {
        var repository = new FakeBetRepository();
        repository.AddMatch(createMatch(1, FixedNowUtc.AddMinutes(1)));
        var service = createService(repository);

        var result = await service.CreateAsync(10, new CreateBetRequest(1, 2, 1));

        Assert.True(result.Succeeded);
        Assert.Single(repository.Bets);
        Assert.Equal(0, result.Value!.PointsEarned);
        Assert.Equal(10, repository.Bets[0].UserId);
        Assert.Equal(2, repository.Bets[0].HomeGoalsPrediction);
    }

    [Fact]
    public async Task CreateAsync_WhenWindowIsClosed_ReturnsBusinessError()
    {
        var repository = new FakeBetRepository();
        repository.AddMatch(createMatch(1, FixedNowUtc));
        var service = createService(repository);

        var result = await service.CreateAsync(10, new CreateBetRequest(1, 2, 1));

        Assert.False(result.Succeeded);
        Assert.Equal(BetErrorCode.BettingWindowClosed, result.ErrorCode);
        Assert.Empty(repository.Bets);
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicate_ReturnsConflictError()
    {
        var repository = new FakeBetRepository();
        repository.AddMatch(createMatch(1, FixedNowUtc.AddMinutes(1)));
        repository.AddBet(createBet(5, userId: 10, matchId: 1));
        var service = createService(repository);

        var result = await service.CreateAsync(10, new CreateBetRequest(1, 2, 1));

        Assert.False(result.Succeeded);
        Assert.Equal(BetErrorCode.DuplicateBet, result.ErrorCode);
        Assert.Single(repository.Bets);
    }

    [Fact]
    public async Task UpdateAsync_WhenBetBelongsToUserAndWindowIsOpen_UpdatesPredictions()
    {
        var repository = new FakeBetRepository();
        var match = createMatch(1, FixedNowUtc.AddMinutes(1));
        repository.AddMatch(match);
        repository.AddBet(createBet(5, userId: 10, matchId: 1, match));
        var service = createService(repository);

        var result = await service.UpdateAsync(10, 5, new UpdateBetRequest(3, 2));

        Assert.True(result.Succeeded);
        Assert.Equal(3, repository.Bets[0].HomeGoalsPrediction);
        Assert.Equal(2, repository.Bets[0].AwayGoalsPrediction);
        Assert.Equal(0, repository.Bets[0].PointsEarned);
    }

    [Fact]
    public async Task UpdateAsync_WhenWindowIsClosed_ReturnsBusinessError()
    {
        var repository = new FakeBetRepository();
        var match = createMatch(1, FixedNowUtc);
        repository.AddMatch(match);
        repository.AddBet(createBet(5, userId: 10, matchId: 1, match));
        var service = createService(repository);

        var result = await service.UpdateAsync(10, 5, new UpdateBetRequest(3, 2));

        Assert.False(result.Succeeded);
        Assert.Equal(BetErrorCode.BettingWindowClosed, result.ErrorCode);
        Assert.Equal(1, repository.Bets[0].HomeGoalsPrediction);
        Assert.Equal(0, repository.Bets[0].AwayGoalsPrediction);
    }

    [Fact]
    public async Task UpdateAsync_WhenBetBelongsToAnotherUser_ReturnsNotFound()
    {
        var repository = new FakeBetRepository();
        var match = createMatch(1, FixedNowUtc.AddMinutes(1));
        repository.AddMatch(match);
        repository.AddBet(createBet(5, userId: 20, matchId: 1, match));
        var service = createService(repository);

        var result = await service.UpdateAsync(10, 5, new UpdateBetRequest(3, 2));

        Assert.False(result.Succeeded);
        Assert.Equal(BetErrorCode.BetNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task ListMineAsync_ReturnsOnlyAuthenticatedUserBets()
    {
        var repository = new FakeBetRepository();
        var newerMatch = createMatch(1, FixedNowUtc.AddMinutes(1), FixedNowUtc.AddDays(2));
        var olderMatch = createMatch(2, FixedNowUtc.AddMinutes(1), FixedNowUtc.AddDays(1));
        repository.AddMatch(newerMatch);
        repository.AddMatch(olderMatch);
        repository.AddBet(createBet(1, userId: 10, matchId: 2, olderMatch));
        repository.AddBet(createBet(2, userId: 20, matchId: 1, newerMatch));
        repository.AddBet(createBet(3, userId: 10, matchId: 1, newerMatch));
        var service = createService(repository);

        var bets = await service.ListMineAsync(10);

        Assert.Equal(2, bets.Count);
        Assert.Equal([1, 2], bets.Select(bet => bet.Match.Id).ToArray());
        Assert.All(bets, bet => Assert.NotEqual(2, bet.Id));
    }

    [Fact]
    public async Task GetVisibilityAsync_WhenUserIsNew_ReturnsVisibleByDefault()
    {
        var repository = new FakeBetRepository();
        repository.AddUser(createUser(10));
        var service = createService(repository);

        var result = await service.GetVisibilityAsync(10);

        Assert.True(result.Succeeded);
        Assert.True(result.Value!.ShowBetsPublicly);
    }

    [Fact]
    public async Task UpdateVisibilityAsync_SavesPrivatePreference()
    {
        var repository = new FakeBetRepository();
        var user = createUser(10);
        repository.AddUser(user);
        var service = createService(repository);

        var result = await service.UpdateVisibilityAsync(10, new UpdateBetVisibilityRequest(false));

        Assert.True(result.Succeeded);
        Assert.False(result.Value!.ShowBetsPublicly);
        Assert.False(user.ShowBetsPublicly);
    }

    [Fact]
    public async Task ListPublicAsync_WhenRequesterIsHidden_ReturnsForbidden()
    {
        var repository = new FakeBetRepository();
        repository.AddUser(createUser(10, showBetsPublicly: false));
        var service = createService(repository);

        var result = await service.ListPublicAsync(10);

        Assert.False(result.Succeeded);
        Assert.Equal(BetErrorCode.PublicBetsNotAllowed, result.ErrorCode);
    }

    [Fact]
    public async Task ListPublicAsync_ReturnsOnlyPublicUsersAndFiltersByMatch()
    {
        var repository = new FakeBetRepository();
        var requester = createUser(10, showBetsPublicly: true, name: "Requester");
        var hiddenUser = createUser(20, showBetsPublicly: false, name: "Hidden");
        var publicUser = createUser(30, showBetsPublicly: true, name: "Public");
        var firstMatch = createMatch(1, FixedNowUtc.AddMinutes(1));
        var secondMatch = createMatch(2, FixedNowUtc.AddMinutes(1));
        repository.AddUser(requester);
        repository.AddUser(hiddenUser);
        repository.AddUser(publicUser);
        repository.AddMatch(firstMatch);
        repository.AddMatch(secondMatch);
        repository.AddBet(createBet(1, requester.Id, firstMatch.Id, firstMatch, requester));
        repository.AddBet(createBet(2, hiddenUser.Id, firstMatch.Id, firstMatch, hiddenUser));
        repository.AddBet(createBet(3, publicUser.Id, firstMatch.Id, firstMatch, publicUser));
        repository.AddBet(createBet(4, publicUser.Id, secondMatch.Id, secondMatch, publicUser));
        var service = createService(repository);

        var result = await service.ListPublicAsync(requester.Id, firstMatch.Id);

        Assert.True(result.Succeeded);
        Assert.Equal([requester.Id, publicUser.Id], result.Value!.Select(bet => bet.UserId).OrderBy(id => id).ToArray());
        Assert.All(result.Value!, bet => Assert.Equal(firstMatch.Id, bet.MatchId));
        Assert.Contains(result.Value!, bet => bet.UserId == requester.Id && bet.IsCurrentUser);
        Assert.DoesNotContain(result.Value!, bet => bet.UserId == hiddenUser.Id);
    }

    [Fact]
    public void CreateBetRequestValidator_WhenGoalsAreNegative_ReturnsErrors()
    {
        var validator = new CreateBetRequestValidator();

        var result = validator.Validate(new CreateBetRequest(0, -1, -1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "MatchId");
        Assert.Contains(result.Errors, error => error.PropertyName == "HomeGoalsPrediction");
        Assert.Contains(result.Errors, error => error.PropertyName == "AwayGoalsPrediction");
    }

    private static BetsService createService(FakeBetRepository repository)
    {
        return new BetsService(repository, new FixedUtcClock(FixedNowUtc));
    }

    private static Match createMatch(
        int id,
        DateTime allowBetUntil,
        DateTime? matchDate = null)
    {
        var homeTeam = new Team { Id = 1, Name = "Brazil", Code = "BRA", FlagUrl = "https://example.test/bra.svg" };
        var awayTeam = new Team { Id = 2, Name = "Germany", Code = "GER", FlagUrl = "https://example.test/ger.svg" };

        return new Match
        {
            Id = id,
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            MatchDate = matchDate ?? FixedNowUtc.AddDays(1),
            Stage = Stage.Groups,
            Status = MatchStatus.Scheduled,
            AllowBetUntil = allowBetUntil
        };
    }

    private static User createUser(
        int id,
        bool showBetsPublicly = true,
        string? name = null)
    {
        return new User
        {
            Id = id,
            Name = name ?? $"User {id}",
            Email = $"user{id}@example.com",
            PasswordHash = "hash",
            ShowBetsPublicly = showBetsPublicly,
            CreatedAt = FixedNowUtc
        };
    }

    private static Bet createBet(
        int id,
        int userId,
        int matchId,
        Match? match = null,
        User? user = null)
    {
        return new Bet
        {
            Id = id,
            UserId = userId,
            MatchId = matchId,
            Match = match,
            User = user,
            HomeGoalsPrediction = 1,
            AwayGoalsPrediction = 0,
            PointsEarned = 0,
            CreatedAt = FixedNowUtc.AddMinutes(-10)
        };
    }

    private sealed class FakeBetRepository : IBetRepository
    {
        private readonly List<Match> matches = [];
        private readonly List<User> users = [];

        public List<Bet> Bets { get; } = [];

        public void AddUser(User user)
        {
            users.Add(user);
        }

        public void AddMatch(Match match)
        {
            matches.Add(match);
        }

        public void AddBet(Bet bet)
        {
            bet.Match ??= matches.Single(match => match.Id == bet.MatchId);
            bet.User ??= users.SingleOrDefault(user => user.Id == bet.UserId);
            Bets.Add(bet);
        }

        public Task<Match?> FindMatchByIdAsync(
            int matchId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(matches.SingleOrDefault(match => match.Id == matchId));
        }

        public Task<User?> FindUserByIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(users.SingleOrDefault(user => user.Id == userId));
        }

        public Task<Bet?> FindByUserAndMatchAsync(
            int userId,
            int matchId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Bets.SingleOrDefault(bet => bet.UserId == userId && bet.MatchId == matchId));
        }

        public Task<Bet?> FindByIdAndUserAsync(
            int betId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Bets.SingleOrDefault(bet => bet.Id == betId && bet.UserId == userId));
        }

        public Task<bool> MatchExistsAsync(
            int matchId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(matches.Any(match => match.Id == matchId));
        }

        public Task<IReadOnlyList<Bet>> ListByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Bet> result = Bets
                .Where(bet => bet.UserId == userId)
                .OrderByDescending(bet => bet.Match!.MatchDate)
                .ThenByDescending(bet => bet.CreatedAt)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Bet>> ListPublicAsync(
            int? matchId = null,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Bet> result = Bets
                .Where(bet => bet.User?.ShowBetsPublicly == true)
                .Where(bet => !matchId.HasValue || bet.MatchId == matchId.Value)
                .OrderBy(bet => bet.MatchId)
                .ThenBy(bet => bet.User!.Name)
                .ThenBy(bet => bet.UserId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task AddAsync(Bet bet, CancellationToken cancellationToken = default)
        {
            bet.Id = Bets.Count == 0 ? 1 : Bets.Max(existing => existing.Id) + 1;
            Bets.Add(bet);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FixedUtcClock(DateTime utcNow) : IUtcClock
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
