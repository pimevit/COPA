using System.Net;
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

namespace BolaoCopa.Tests.Matches;

public sealed class MatchesApiTests : IClassFixture<MatchesApiTests.MatchesApiFactory>
{
    private readonly MatchesApiFactory factory;

    public MatchesApiTests(MatchesApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task GetMatches_ReturnsMatchesOrderedByMatchDate()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/matches");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var matches = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, matches.Length);
        Assert.Equal(2, matches[0].GetProperty("id").GetInt32());
        Assert.Equal(1, matches[1].GetProperty("id").GetInt32());
        Assert.Equal("Brazil", matches[1].GetProperty("homeTeam").GetProperty("name").GetString());
        Assert.False(matches[1].GetProperty("isBettingOpen").GetBoolean());
        Assert.False(matches[2].GetProperty("isBettingOpen").GetBoolean());
    }

    [Fact]
    public async Task GetMatches_WithValidFilters_ReturnsFilteredMatches()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/matches?stage=Groups&status=Scheduled");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var matches = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, matches.Length);
        Assert.All(matches, match =>
        {
            Assert.Equal("Groups", match.GetProperty("stage").GetString());
            Assert.Equal("Scheduled", match.GetProperty("status").GetString());
        });
    }

    [Fact]
    public async Task GetMatches_WithInvalidFilter_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/matches?stage=Unknown");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Invalid Stage filter", body);
    }

    [Fact]
    public async Task GetMatchById_WhenExists_ReturnsMatchDetail()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/matches/3");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var match = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, match.GetProperty("id").GetInt32());
        Assert.Equal("Final", match.GetProperty("stage").GetString());
        Assert.Equal(2, match.GetProperty("homeGoals").GetInt32());
        Assert.Equal(1, match.GetProperty("awayGoals").GetInt32());
        Assert.Equal("Argentina", match.GetProperty("awayTeam").GetProperty("name").GetString());
        Assert.False(match.GetProperty("isBettingOpen").GetBoolean());
    }

    [Fact]
    public async Task GetMatchById_WhenMissing_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/matches/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public sealed class MatchesApiFactory : WebApplicationFactory<Program>
    {
        private static readonly DateTime FixedNowUtc = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        private readonly string databaseName = $"matches-api-{Guid.NewGuid():N}";

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
            var france = new Team { Id = 3, Name = "France", Code = "FRA", FlagUrl = "https://example.test/fra.svg" };
            var argentina = new Team { Id = 4, Name = "Argentina", Code = "ARG", FlagUrl = "https://example.test/arg.svg" };

            dbContext.Teams.AddRange(brazil, germany, france, argentina);
            dbContext.Matches.AddRange(
                new Match
                {
                    Id = 1,
                    HomeTeamId = brazil.Id,
                    AwayTeamId = germany.Id,
                    MatchDate = new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Scheduled,
                    AllowBetUntil = FixedNowUtc
                },
                new Match
                {
                    Id = 2,
                    HomeTeamId = france.Id,
                    AwayTeamId = argentina.Id,
                    MatchDate = new DateTime(2026, 6, 11, 18, 0, 0, DateTimeKind.Utc),
                    Stage = Stage.Groups,
                    Status = MatchStatus.Scheduled,
                    AllowBetUntil = FixedNowUtc.AddHours(1)
                },
                new Match
                {
                    Id = 3,
                    HomeTeamId = brazil.Id,
                    AwayTeamId = argentina.Id,
                    HomeGoals = 2,
                    AwayGoals = 1,
                    MatchDate = new DateTime(2026, 7, 19, 18, 0, 0, DateTimeKind.Utc),
                    Stage = Stage.Final,
                    Status = MatchStatus.Finished,
                    AllowBetUntil = FixedNowUtc.AddMinutes(-1)
                });

            dbContext.SaveChanges();
        }

        private sealed class FixedUtcClock(DateTime utcNow) : IUtcClock
        {
            public DateTime UtcNow { get; } = utcNow;
        }
    }
}
