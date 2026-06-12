using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

namespace BolaoCopa.Tests.Admin;

public sealed class AdminMaintenanceApiTests
{
    public static TheoryData<string, HttpMethod> ProtectedEndpoints => new()
    {
        { "/admin/maintenance/teams/brasileirao-serie-a-2026", HttpMethod.Post },
        { "/admin/maintenance/teams/world-cup-2026", HttpMethod.Post },
        { "/admin/maintenance/recalculate-points", HttpMethod.Post },
        { "/admin/maintenance/application-data", HttpMethod.Delete }
    };

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task MaintenanceEndpoint_WhenAnonymous_ReturnsUnauthorized(string endpoint, HttpMethod method)
    {
        using var factory = new AdminMaintenanceApiFactory();
        var client = factory.CreateClient();

        using var response = await client.SendAsync(new HttpRequestMessage(method, endpoint));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task MaintenanceEndpoint_WhenUserIsNotAdmin_ReturnsForbidden(string endpoint, HttpMethod method)
    {
        using var factory = new AdminMaintenanceApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com");

        using var response = await client.SendAsync(new HttpRequestMessage(method, endpoint));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ImportBrasileiraoTeams_WhenAdmin_IsIdempotent()
    {
        using var factory = new AdminMaintenanceApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var firstResponse = await client.PostAsync("/admin/maintenance/teams/brasileirao-serie-a-2026", null);
        var firstResult = await readResultAsync(firstResponse);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal("brasileirao-serie-a-2026", firstResult.Action);
        Assert.Equal(20, firstResult.InsertedTeams);
        Assert.Equal(0, firstResult.UpdatedTeams);

        var secondResponse = await client.PostAsync("/admin/maintenance/teams/brasileirao-serie-a-2026", null);
        var secondResult = await readResultAsync(secondResponse);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(0, secondResult.InsertedTeams);
        Assert.Equal(0, secondResult.UpdatedTeams);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(20, await dbContext.Teams.CountAsync());
        Assert.Contains(await dbContext.Teams.ToListAsync(), team => team.Code == "FLA" && team.Name == "Flamengo");
    }

    [Fact]
    public async Task ImportWorldCupTeams_WhenAdmin_IsIdempotent()
    {
        using var factory = new AdminMaintenanceApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");

        var firstResponse = await client.PostAsync("/admin/maintenance/teams/world-cup-2026", null);
        var firstResult = await readResultAsync(firstResponse);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal("world-cup-2026", firstResult.Action);
        Assert.Equal(48, firstResult.InsertedTeams);
        Assert.Equal(0, firstResult.UpdatedTeams);

        var secondResponse = await client.PostAsync("/admin/maintenance/teams/world-cup-2026", null);
        var secondResult = await readResultAsync(secondResponse);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(0, secondResult.InsertedTeams);
        Assert.Equal(0, secondResult.UpdatedTeams);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(48, await dbContext.Teams.CountAsync());
        var teams = await dbContext.Teams.ToListAsync();
        Assert.Contains(teams, team => team.Code == "CAN" && team.Name == "Canadá");
        Assert.Contains(teams, team => team.Code == "BRA" && team.Name == "Brasil");
        Assert.Contains(teams, team => team.Code == "GER" && team.Name == "Alemanha");
        Assert.Contains(teams, team => team.Code == "PAR" && team.Name == "Paraguai");
    }

    [Fact]
    public async Task ClearApplicationData_WhenAdmin_DeletesBetsMatchesAndTeamsButPreservesUsers()
    {
        using var factory = new AdminMaintenanceApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");
        await seedApplicationDataAsync(factory);

        var response = await client.DeleteAsync("/admin/maintenance/application-data");
        var result = await readResultAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application-data-reset", result.Action);
        Assert.Equal(1, result.DeletedBets);
        Assert.Equal(1, result.DeletedMatches);
        Assert.Equal(2, result.DeletedTeams);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Empty(await dbContext.Bets.ToListAsync());
        Assert.Empty(await dbContext.Matches.ToListAsync());
        Assert.Empty(await dbContext.Teams.ToListAsync());
    }

    [Fact]
    public async Task RecalculatePoints_WhenAdmin_ReplacesPointsForFinishedMatches()
    {
        using var factory = new AdminMaintenanceApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com");
        await seedFinishedMatchWithOutdatedPointsAsync(factory);

        var response = await client.PostAsync("/admin/maintenance/recalculate-points", null);
        var result = await readResultAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("points-recalculated", result.Action);
        Assert.Equal(1, result.RecalculatedMatches);
        Assert.Equal(1, result.RecalculatedBets);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bet = await dbContext.Bets.SingleAsync();

        Assert.Equal(3, bet.PointsEarned);
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
        var loginBody = await login.Content.ReadFromJsonAsync<AuthTokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);
    }

    private static async Task<AdminMaintenanceResponseDto> readResultAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<AdminMaintenanceResponseDto>()
            ?? throw new InvalidOperationException("Maintenance response body could not be parsed.");
    }

    private static async Task seedApplicationDataAsync(AdminMaintenanceApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var homeTeam = new Team { Name = "Brazil", Code = "BRA", FlagUrl = "https://example.test/bra.svg" };
        var awayTeam = new Team { Name = "Argentina", Code = "ARG", FlagUrl = "https://example.test/arg.svg" };
        var match = new Match
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            MatchDate = new DateTime(2026, 6, 11, 19, 0, 0, DateTimeKind.Utc),
            Stage = Stage.Groups,
            Status = MatchStatus.Scheduled,
            AllowBetUntil = new DateTime(2026, 6, 11, 18, 45, 0, DateTimeKind.Utc)
        };
        var bet = new Bet
        {
            User = user,
            Match = match,
            HomeGoalsPrediction = 2,
            AwayGoalsPrediction = 1,
            PointsEarned = 0,
            CreatedAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        dbContext.Bets.Add(bet);
        await dbContext.SaveChangesAsync();
    }

    private static async Task seedFinishedMatchWithOutdatedPointsAsync(AdminMaintenanceApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync();
        var homeTeam = new Team { Name = "Brazil", Code = "BRA", FlagUrl = "https://example.test/bra.svg" };
        var awayTeam = new Team { Name = "Argentina", Code = "ARG", FlagUrl = "https://example.test/arg.svg" };
        var match = new Match
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeGoals = 2,
            AwayGoals = 0,
            MatchDate = new DateTime(2026, 6, 11, 19, 0, 0, DateTimeKind.Utc),
            Stage = Stage.Groups,
            Status = MatchStatus.Finished,
            AllowBetUntil = new DateTime(2026, 6, 11, 18, 45, 0, DateTimeKind.Utc)
        };
        var bet = new Bet
        {
            User = user,
            Match = match,
            HomeGoalsPrediction = 2,
            AwayGoalsPrediction = 1,
            PointsEarned = 2,
            CreatedAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        dbContext.Bets.Add(bet);
        await dbContext.SaveChangesAsync();
    }

    private sealed record AuthTokenResponse(string AccessToken);

    private sealed record AdminMaintenanceResponseDto(
        string Action,
        int InsertedTeams,
        int UpdatedTeams,
        int DeletedBets,
        int DeletedMatches,
        int DeletedTeams,
        int RecalculatedMatches,
        int RecalculatedBets);

    private sealed class AdminMaintenanceApiFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"admin-maintenance-api-{Guid.NewGuid():N}";

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

                services.AddDbContext<AppDbContext>(options =>
                {
                    options
                        .UseInMemoryDatabase(databaseName)
                        .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        }
    }
}
