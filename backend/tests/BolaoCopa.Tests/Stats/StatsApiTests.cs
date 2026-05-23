using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BolaoCopa.Tests.Stats;

public sealed class StatsApiTests
{
    [Fact]
    public async Task GetStatsMe_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new StatsApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/stats/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStatsMe_ReturnsAuthenticatedUserStats()
    {
        using var factory = new StatsApiFactory();
        var client = factory.CreateClient();
        await createUsersAndBetsAsync(client, factory);

        await authenticateAsync(client, "alice@example.com");
        var response = await client.GetAsync("/stats/me");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var stats = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(7, stats.GetProperty("totalPoints").GetInt32());
        Assert.Equal(2, stats.GetProperty("exactScoreCount").GetInt32());
        Assert.Equal(2, stats.GetProperty("winnerHitCount").GetInt32());
        Assert.Equal(1, stats.GetProperty("hitPercentage").GetDouble());
        Assert.Equal(2, stats.GetProperty("bestHitStreak").GetInt32());
        Assert.Equal("/bets/me", stats.GetProperty("historyEndpoint").GetString());
    }

    [Fact]
    public async Task GetStatsMe_DoesNotIncludeAnotherUserBets()
    {
        using var factory = new StatsApiFactory();
        var client = factory.CreateClient();
        await createUsersAndBetsAsync(client, factory);

        await authenticateAsync(client, "bob@example.com");
        var response = await client.GetAsync("/stats/me");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var stats = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5, stats.GetProperty("totalPoints").GetInt32());
        Assert.Equal(1, stats.GetProperty("exactScoreCount").GetInt32());
        Assert.Equal(1, stats.GetProperty("winnerHitCount").GetInt32());
    }

    [Fact]
    public async Task GetStatsMe_WhenUserHasNoEvaluatedBets_ReturnsZeroMetrics()
    {
        using var factory = new StatsApiFactory();
        var client = factory.CreateClient();
        await registerAsync(client, "Empty", "empty@example.com");

        await authenticateAsync(client, "empty@example.com");
        var response = await client.GetAsync("/stats/me");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var stats = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, stats.GetProperty("totalPoints").GetInt32());
        Assert.Equal(0, stats.GetProperty("exactScoreCount").GetInt32());
        Assert.Equal(0, stats.GetProperty("winnerHitCount").GetInt32());
        Assert.Equal(0, stats.GetProperty("hitPercentage").GetDouble());
        Assert.Equal(0, stats.GetProperty("bestHitStreak").GetInt32());
    }

    [Fact]
    public async Task GetStatsMe_IgnoresMatchesWithoutResult()
    {
        using var factory = new StatsApiFactory();
        var client = factory.CreateClient();
        await createUsersAndBetsAsync(client, factory);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alice = await dbContext.Users.SingleAsync(user => user.Email == "alice@example.com");
        dbContext.Bets.Add(new Bet
        {
            UserId = alice.Id,
            MatchId = 3,
            HomeGoalsPrediction = 9,
            AwayGoalsPrediction = 9,
            PointsEarned = 99,
            CreatedAt = new DateTime(2026, 6, 3, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        await authenticateAsync(client, "alice@example.com");
        var response = await client.GetAsync("/stats/me");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var stats = document.RootElement;

        Assert.Equal(7, stats.GetProperty("totalPoints").GetInt32());
    }

    private static async Task createUsersAndBetsAsync(HttpClient client, StatsApiFactory factory)
    {
        await registerAsync(client, "Alice", "alice@example.com");
        await registerAsync(client, "Bob", "bob@example.com");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = await dbContext.Users.ToDictionaryAsync(user => user.Email);

        dbContext.Bets.AddRange(
            createBet(users["alice@example.com"].Id, matchId: 1, points: 5, createdOffset: 1),
            createBet(users["alice@example.com"].Id, matchId: 2, points: 2, createdOffset: 2),
            createBet(users["bob@example.com"].Id, matchId: 1, points: 5, createdOffset: 3));

        await dbContext.SaveChangesAsync();
    }

    private static Bet createBet(int userId, int matchId, int points, int createdOffset)
    {
        return new Bet
        {
            UserId = userId,
            MatchId = matchId,
            HomeGoalsPrediction = matchId == 1 ? 2 : 1,
            AwayGoalsPrediction = matchId == 1 ? 0 : 1,
            PointsEarned = points,
            CreatedAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc).AddMinutes(createdOffset)
        };
    }

    private static async Task registerAsync(HttpClient client, string name, string email)
    {
        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name,
            email,
            password = "secret123"
        });

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict,
            $"Unexpected register status: {response.StatusCode}");
    }

    private static async Task authenticateAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "secret123"
        });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public sealed class StatsApiFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"stats-api-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                });

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                seed(dbContext);
            });
        }

        private static void seed(AppDbContext dbContext)
        {
            var brazil = new Team { Id = 1, Name = "Brazil", Code = "BRA", FlagUrl = "https://example.test/bra.svg" };
            var germany = new Team { Id = 2, Name = "Germany", Code = "GER", FlagUrl = "https://example.test/ger.svg" };

            dbContext.Teams.AddRange(brazil, germany);
            dbContext.Matches.AddRange(
                new Match
                {
                    Id = 1,
                    HomeTeamId = brazil.Id,
                    AwayTeamId = germany.Id,
                    HomeGoals = 2,
                    AwayGoals = 0,
                    MatchDate = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Finished,
                    AllowBetUntil = new DateTime(2026, 6, 10, 11, 45, 0, DateTimeKind.Utc)
                },
                new Match
                {
                    Id = 2,
                    HomeTeamId = germany.Id,
                    AwayTeamId = brazil.Id,
                    HomeGoals = 1,
                    AwayGoals = 1,
                    MatchDate = new DateTime(2026, 6, 11, 12, 0, 0, DateTimeKind.Utc),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Finished,
                    AllowBetUntil = new DateTime(2026, 6, 11, 11, 45, 0, DateTimeKind.Utc)
                },
                new Match
                {
                    Id = 3,
                    HomeTeamId = brazil.Id,
                    AwayTeamId = germany.Id,
                    MatchDate = new DateTime(2026, 6, 12, 12, 0, 0, DateTimeKind.Utc),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Scheduled,
                    AllowBetUntil = new DateTime(2026, 6, 12, 11, 45, 0, DateTimeKind.Utc)
                });

            dbContext.SaveChanges();
        }
    }
}
