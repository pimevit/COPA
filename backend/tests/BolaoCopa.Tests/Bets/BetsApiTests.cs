using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BolaoCopa.Tests.Bets;

public sealed class BetsApiTests
{
    [Fact]
    public async Task BetsEndpoints_WithoutToken_ReturnUnauthorized()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();

        var post = await client.PostAsJsonAsync("/bets", new
        {
            matchId = 1,
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });
        var put = await client.PutAsJsonAsync("/bets/1", new
        {
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });
        var get = await client.GetAsync("/bets/me");

        Assert.Equal(HttpStatusCode.Unauthorized, post.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, put.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, get.StatusCode);
    }

    [Fact]
    public async Task PostBet_WhenWindowIsOpen_CreatesBet()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");

        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId = 1,
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var bet = document.RootElement;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(1, bet.GetProperty("matchId").GetInt32());
        Assert.Equal(2, bet.GetProperty("homeGoalsPrediction").GetInt32());
        Assert.Equal(0, bet.GetProperty("pointsEarned").GetInt32());
        Assert.Equal("Brazil", bet.GetProperty("match").GetProperty("homeTeam").GetProperty("name").GetString());
    }

    [Fact]
    public async Task PostBet_WhenWindowIsClosed_ReturnsUnprocessableEntity()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");

        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId = 2,
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Betting window is closed", body);
    }

    [Fact]
    public async Task PostBet_WhenDuplicate_ReturnsConflict()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");

        await client.PostAsJsonAsync("/bets", new
        {
            matchId = 1,
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });
        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId = 1,
            homeGoalsPrediction = 3,
            awayGoalsPrediction = 0
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PutBet_WhenOwnBetAndWindowIsOpen_UpdatesBet()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");
        var betId = await createBetAsync(client, matchId: 1);

        var response = await client.PutAsJsonAsync($"/bets/{betId}", new
        {
            homeGoalsPrediction = 3,
            awayGoalsPrediction = 2
        });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var bet = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, bet.GetProperty("homeGoalsPrediction").GetInt32());
        Assert.Equal(2, bet.GetProperty("awayGoalsPrediction").GetInt32());
    }

    [Fact]
    public async Task PutBet_WhenAnotherUserOwnsBet_ReturnsNotFound()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");
        var betId = await createBetAsync(client, matchId: 1);

        await authenticateAsync(client, "user2@example.com");
        var response = await client.PutAsJsonAsync($"/bets/{betId}", new
        {
            homeGoalsPrediction = 3,
            awayGoalsPrediction = 2
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutBet_WhenWindowIsClosed_ReturnsUnprocessableEntity()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");
        var betId = await createBetAsync(client, matchId: 3);

        factory.CloseMatchWindow(3);

        var response = await client.PutAsJsonAsync($"/bets/{betId}", new
        {
            homeGoalsPrediction = 3,
            awayGoalsPrediction = 2
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetBetsMe_ReturnsOnlyAuthenticatedUserBets()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");
        var user1BetId = await createBetAsync(client, matchId: 1);

        await authenticateAsync(client, "user2@example.com");
        await createBetAsync(client, matchId: 1);

        await authenticateAsync(client, "user1@example.com");
        var response = await client.GetAsync("/bets/me");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var bets = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bet = Assert.Single(bets);
        Assert.Equal(user1BetId, bet.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task PostBet_WhenGoalsAreNegative_ReturnsBadRequest()
    {
        using var factory = new BetsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user1@example.com");

        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId = 1,
            homeGoalsPrediction = -1,
            awayGoalsPrediction = 0
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("HomeGoalsPrediction", body);
    }

    private static async Task<int> createBetAsync(HttpClient client, int matchId)
    {
        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId,
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return document.RootElement.GetProperty("id").GetInt32();
    }

    private static async Task authenticateAsync(HttpClient client, string email)
    {
        var password = "secret123";

        var register = await client.PostAsJsonAsync("/auth/register", new
        {
            name = email.Split('@')[0],
            email,
            password
        });

        Assert.True(
            register.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict,
            $"Unexpected register status: {register.StatusCode}");

        var login = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password
        });
        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed class BetsApiFactory : WebApplicationFactory<Program>
    {
        private static readonly DateTime FixedNowUtc = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        private readonly string databaseName = $"bets-api-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IUtcClock>();

                services.AddSingleton<IUtcClock>(new FixedUtcClock(FixedNowUtc));
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
                    MatchDate = FixedNowUtc.AddDays(1),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Scheduled,
                    AllowBetUntil = FixedNowUtc.AddMinutes(1)
                },
                new Match
                {
                    Id = 2,
                    HomeTeamId = brazil.Id,
                    AwayTeamId = germany.Id,
                    MatchDate = FixedNowUtc.AddDays(2),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Scheduled,
                    AllowBetUntil = FixedNowUtc
                },
                new Match
                {
                    Id = 3,
                    HomeTeamId = brazil.Id,
                    AwayTeamId = germany.Id,
                    MatchDate = FixedNowUtc.AddDays(3),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Scheduled,
                    AllowBetUntil = FixedNowUtc.AddMinutes(1)
                });

            dbContext.SaveChanges();
        }

        public void CloseMatchWindow(int matchId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var match = dbContext.Matches.Single(match => match.Id == matchId);
            match.AllowBetUntil = FixedNowUtc;
            dbContext.SaveChanges();
        }

        private sealed class FixedUtcClock(DateTime utcNow) : IUtcClock
        {
            public DateTime UtcNow { get; } = utcNow;
        }
    }
}
