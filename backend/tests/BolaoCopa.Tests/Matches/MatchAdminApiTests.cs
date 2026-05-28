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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BolaoCopa.Tests.Matches;

public sealed class MatchAdminApiTests
{
    [Fact]
    public async Task GetTeams_WhenAdmin_ReturnsTeams()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.GetAsync("/teams");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var teams = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, teams.Length);
        Assert.Equal("Brazil", teams[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetTeams_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com");

        var response = await client.GetAsync("/teams");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostMatch_WhenAdminAndValid_CreatesScheduledMatch()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var matchDate = new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc);
        var response = await client.PostAsJsonAsync("/matches", new
        {
            homeTeamId = 1,
            awayTeamId = 2,
            matchDate,
            stage = "Groups"
        });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var match = document.RootElement;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Scheduled", match.GetProperty("status").GetString());
        Assert.True(match.GetProperty("isBettingOpen").GetBoolean());
        Assert.False(match.GetProperty("isBettingLocked").GetBoolean());
        Assert.Equal("Brazil", match.GetProperty("homeTeam").GetProperty("name").GetString());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var created = await dbContext.Matches.SingleAsync(match => match.MatchDate == matchDate);

        Assert.Equal(MatchStatus.Scheduled, created.Status);
        Assert.Null(created.HomeGoals);
        Assert.Null(created.AwayGoals);
        Assert.Equal(matchDate.AddMinutes(-15), created.AllowBetUntil);
    }

    [Fact]
    public async Task PostMatch_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com");

        var response = await client.PostAsJsonAsync("/matches", new
        {
            homeTeamId = 1,
            awayTeamId = 2,
            matchDate = new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc),
            stage = "Groups"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostMatch_WhenTeamsAreSame_ReturnsBadRequest()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.PostAsJsonAsync("/matches", new
        {
            homeTeamId = 1,
            awayTeamId = 1,
            matchDate = new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc),
            stage = "Groups"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostMatch_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.PostAsJsonAsync("/matches", new
        {
            homeTeamId = 1,
            awayTeamId = 999,
            matchDate = new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc),
            stage = "Groups"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostMatch_WhenDuplicate_ReturnsConflict()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.PostAsJsonAsync("/matches", new
        {
            homeTeamId = 1,
            awayTeamId = 2,
            matchDate = new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc),
            stage = "Groups"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostMatch_WhenStageIsInvalid_ReturnsBadRequest()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.PostAsJsonAsync("/matches", new
        {
            homeTeamId = 1,
            awayTeamId = 2,
            matchDate = new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc),
            stage = "Unknown"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMatch_WhenAdminAndMatchHasNoBets_DeletesMatch()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "admin@example.com");
        var response = await client.DeleteAsync("/matches/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(0, await dbContext.Matches.CountAsync());
        Assert.Equal(0, await dbContext.Bets.CountAsync());
    }

    [Fact]
    public async Task DeleteMatch_WhenAdminAndMatchHasBetsWithoutDeleteBets_ReturnsConflict()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "user@example.com");
        await createBetAsync(client, matchId: 1);

        await authenticateAsync(client, "admin@example.com");
        var response = await client.DeleteAsync("/matches/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(1, await dbContext.Matches.CountAsync());
        Assert.Equal(1, await dbContext.Bets.CountAsync());
    }

    [Fact]
    public async Task DeleteMatch_WhenAdminAndDeleteBetsIsTrue_DeletesMatchAndRelatedBets()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "user@example.com");
        await createBetAsync(client, matchId: 1);

        await authenticateAsync(client, "admin@example.com");
        var response = await client.DeleteAsync("/matches/1?deleteBets=true");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(0, await dbContext.Matches.CountAsync());
        Assert.Equal(0, await dbContext.Bets.CountAsync());
    }

    [Fact]
    public async Task DeleteMatch_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com");

        var response = await client.DeleteAsync("/matches/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMatch_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.DeleteAsync("/matches/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBettingLock_WhenAdminLocksOpenMatch_ClosesWindowAndRejectsBetChanges()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "user@example.com");
        var betId = await createBetAsync(client, matchId: 1);

        await authenticateAsync(client, "admin@example.com");
        var lockResponse = await client.PutAsJsonAsync("/matches/1/betting-lock", new { isBettingLocked = true });
        var matchResponse = await client.GetAsync("/matches/1");
        using var document = JsonDocument.Parse(await matchResponse.Content.ReadAsStringAsync());
        var match = document.RootElement;

        Assert.Equal(HttpStatusCode.NoContent, lockResponse.StatusCode);
        Assert.False(match.GetProperty("isBettingOpen").GetBoolean());
        Assert.True(match.GetProperty("isBettingLocked").GetBoolean());

        await authenticateAsync(client, "other@example.com");
        var createResponse = await client.PostAsJsonAsync("/bets", new
        {
            matchId = 1,
            homeGoalsPrediction = 1,
            awayGoalsPrediction = 0
        });

        await authenticateAsync(client, "user@example.com");
        var updateResponse = await client.PutAsJsonAsync($"/bets/{betId}", new
        {
            homeGoalsPrediction = 3,
            awayGoalsPrediction = 1
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateBettingLock_WhenAdminUnlocksOpenWindow_ReopensBettingWindow()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        await client.PutAsJsonAsync("/matches/1/betting-lock", new { isBettingLocked = true });
        var unlockResponse = await client.PutAsJsonAsync("/matches/1/betting-lock", new { isBettingLocked = false });
        var matchResponse = await client.GetAsync("/matches/1");
        using var document = JsonDocument.Parse(await matchResponse.Content.ReadAsStringAsync());
        var match = document.RootElement;

        Assert.Equal(HttpStatusCode.NoContent, unlockResponse.StatusCode);
        Assert.True(match.GetProperty("isBettingOpen").GetBoolean());
        Assert.False(match.GetProperty("isBettingLocked").GetBoolean());
    }

    [Fact]
    public async Task UpdateBettingLock_WhenAutomaticWindowIsClosed_DoesNotReopenBettingWindow()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        factory.CloseMatchWindow(matchId: 1);
        await authenticateAsync(client, "admin@example.com");

        var unlockResponse = await client.PutAsJsonAsync("/matches/1/betting-lock", new { isBettingLocked = false });
        var matchResponse = await client.GetAsync("/matches/1");
        using var document = JsonDocument.Parse(await matchResponse.Content.ReadAsStringAsync());
        var match = document.RootElement;

        Assert.Equal(HttpStatusCode.NoContent, unlockResponse.StatusCode);
        Assert.False(match.GetProperty("isBettingOpen").GetBoolean());
        Assert.False(match.GetProperty("isBettingLocked").GetBoolean());
    }

    [Fact]
    public async Task UpdateBettingLock_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new MatchAdminApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com");

        var response = await client.PutAsJsonAsync("/matches/1/betting-lock", new { isBettingLocked = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<int> createBetAsync(HttpClient client, int matchId)
    {
        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId,
            homeGoalsPrediction = 2,
            awayGoalsPrediction = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

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

    private sealed class MatchAdminApiFactory : WebApplicationFactory<Program>
    {
        private static readonly DateTime FixedNowUtc = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        private readonly string databaseName = $"match-admin-api-{Guid.NewGuid():N}";

        public void CloseMatchWindow(int matchId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var match = dbContext.Matches.Single(match => match.Id == matchId);

            match.AllowBetUntil = FixedNowUtc;
            dbContext.SaveChanges();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:AdminEmails:0"] = "admin@example.com"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IUtcClock>();

                services.AddSingleton<IUtcClock>(new FixedUtcClock(FixedNowUtc));
                services.AddDbContext<AppDbContext>(options =>
                {
                    options
                        .UseInMemoryDatabase(databaseName)
                        .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
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
            dbContext.Matches.Add(new Match
            {
                Id = 1,
                HomeTeamId = brazil.Id,
                AwayTeamId = germany.Id,
                MatchDate = new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc),
                Stage = Stage.Groups,
                Status = MatchStatus.Scheduled,
                AllowBetUntil = new DateTime(2026, 6, 12, 17, 45, 0, DateTimeKind.Utc)
            });

            dbContext.SaveChanges();
        }

        private sealed class FixedUtcClock(DateTime utcNow) : IUtcClock
        {
            public DateTime UtcNow { get; } = utcNow;
        }
    }
}
