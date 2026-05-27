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

public sealed class MatchResultsApiTests
{
    [Fact]
    public async Task PutMatchResult_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutMatchResult_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com");

        var response = await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutMatchResult_WhenValid_UpdatesMatchAndRecalculatesBets()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "user1@example.com");
        await createBetAsync(client, matchId: 1, homePrediction: 2, awayPrediction: 1);

        await authenticateAsync(client, "user2@example.com");
        await createBetAsync(client, matchId: 1, homePrediction: 1, awayPrediction: 0);

        await authenticateAsync(client, "admin@example.com");
        var response = await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var result = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, result.GetProperty("id").GetInt32());
        Assert.Equal("Finished", result.GetProperty("status").GetString());
        Assert.Equal(2, result.GetProperty("recalculatedBets").GetInt32());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = await dbContext.Matches.SingleAsync(match => match.Id == 1);
        var bets = await dbContext.Bets.OrderBy(bet => bet.UserId).ToListAsync();

        Assert.Equal(2, match.HomeGoals);
        Assert.Equal(1, match.AwayGoals);
        Assert.Equal(MatchStatus.Finished, match.Status);
        Assert.Equal([5, 2], bets.Select(bet => bet.PointsEarned).ToArray());
    }

    [Fact]
    public async Task PutMatchResult_WhenResultIsCorrected_ReplacesPoints()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "user1@example.com");
        await createBetAsync(client, matchId: 1, homePrediction: 2, awayPrediction: 1);

        await authenticateAsync(client, "admin@example.com");
        await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });
        await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 0
        });

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bet = await dbContext.Bets.SingleAsync();

        Assert.Equal(2, bet.PointsEarned);
    }

    [Fact]
    public async Task PutMatchResult_WhenSameResultIsSubmittedAgain_IsIdempotent()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();

        await authenticateAsync(client, "user1@example.com");
        await createBetAsync(client, matchId: 1, homePrediction: 2, awayPrediction: 1);

        await authenticateAsync(client, "admin@example.com");
        await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });
        await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(1, await dbContext.Bets.CountAsync());
        Assert.Equal(5, (await dbContext.Bets.SingleAsync()).PointsEarned);
    }

    [Fact]
    public async Task PutMatchResult_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.PutAsJsonAsync("/matches/999/result", new
        {
            homeGoals = 2,
            awayGoals = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutMatchResult_WhenGoalsAreInvalid_ReturnsBadRequest()
    {
        using var factory = new MatchResultsApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var response = await client.PutAsJsonAsync("/matches/1/result", new
        {
            homeGoals = -1,
            awayGoals = 1
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("HomeGoals", body);
    }

    private static async Task createBetAsync(
        HttpClient client,
        int matchId,
        int homePrediction,
        int awayPrediction)
    {
        var response = await client.PostAsJsonAsync("/bets", new
        {
            matchId,
            homeGoalsPrediction = homePrediction,
            awayGoalsPrediction = awayPrediction
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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

    private sealed class MatchResultsApiFactory : WebApplicationFactory<Program>
    {
        private static readonly DateTime FixedNowUtc = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        private readonly string databaseName = $"match-results-api-{Guid.NewGuid():N}";

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
                MatchDate = FixedNowUtc.AddDays(1),
                Stage = Stage.Groups,
                Status = MatchStatus.Scheduled,
                AllowBetUntil = FixedNowUtc.AddMinutes(1)
            });

            dbContext.SaveChanges();
        }

        private sealed class FixedUtcClock(DateTime utcNow) : IUtcClock
        {
            public DateTime UtcNow { get; } = utcNow;
        }
    }
}
