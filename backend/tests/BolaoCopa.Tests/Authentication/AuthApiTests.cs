using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BolaoCopa.Tests.Authentication;

public sealed class AuthApiTests
{
    private const string RefreshCookieName = "bolao-copa-refresh-token";

    [Fact]
    public async Task Login_SetsHttpOnlyRefreshTokenCookie()
    {
        using var factory = new AuthApiFactory();
        var client = createClientWithoutCookies(factory);

        await registerAsync(client);
        var response = await loginAsync(client);
        var refreshCookie = getRefreshCookie(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("httponly", refreshCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/auth", refreshCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", refreshCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithValidCredentials_UpdatesLastLogin()
    {
        using var factory = new AuthApiFactory();
        var client = createClientWithoutCookies(factory);
        var oldLastLoginAtUtc = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

        await registerAsync(client);
        await setLastLoginAsync(factory, "user@example.com", oldLastLoginAtUtc);
        var response = await loginAsync(client);
        var lastLoginAtUtc = await getLastLoginAsync(factory, "user@example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(lastLoginAtUtc);
        Assert.True(lastLoginAtUtc > oldLastLoginAtUtc);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_DoesNotUpdateLastLogin()
    {
        using var factory = new AuthApiFactory();
        var client = createClientWithoutCookies(factory);
        var oldLastLoginAtUtc = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

        await registerAsync(client);
        await setLastLoginAsync(factory, "user@example.com", oldLastLoginAtUtc);
        var response = await client.PostAsJsonAsync("/auth/login", new
        {
            email = "user@example.com",
            password = "wrong123"
        });
        var lastLoginAtUtc = await getLastLoginAsync(factory, "user@example.com");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(oldLastLoginAtUtc, lastLoginAtUtc);
    }

    [Fact]
    public async Task Refresh_WithValidCookie_ReturnsNewAccessTokenAndRotatesRefreshToken()
    {
        using var factory = new AuthApiFactory();
        var client = createClientWithoutCookies(factory);

        await registerAsync(client);
        var login = await loginAsync(client);
        var originalCookieHeader = getRefreshCookieHeader(login);
        var oldLastLoginAtUtc = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        await setLastLoginAsync(factory, "user@example.com", oldLastLoginAtUtc);

        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        refreshRequest.Headers.Add("Cookie", originalCookieHeader);
        var refresh = await client.SendAsync(refreshRequest);
        var rotatedCookieHeader = getRefreshCookieHeader(refresh);
        using var document = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("accessToken").GetString()));
        Assert.Equal("user@example.com", document.RootElement.GetProperty("user").GetProperty("email").GetString());
        Assert.NotEqual(originalCookieHeader, rotatedCookieHeader);
        Assert.True(await getLastLoginAsync(factory, "user@example.com") > oldLastLoginAtUtc);

        using var reusedOldTokenRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        reusedOldTokenRequest.Headers.Add("Cookie", originalCookieHeader);
        var reusedOldToken = await client.SendAsync(reusedOldTokenRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, reusedOldToken.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesOnlyCurrentRefreshToken()
    {
        using var factory = new AuthApiFactory();
        var client = createClientWithoutCookies(factory);

        await registerAsync(client);
        var firstLogin = await loginAsync(client);
        var secondLogin = await loginAsync(client);
        var firstCookieHeader = getRefreshCookieHeader(firstLogin);
        var secondCookieHeader = getRefreshCookieHeader(secondLogin);

        using var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        logoutRequest.Headers.Add("Cookie", firstCookieHeader);
        var logout = await client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        using var firstRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        firstRefreshRequest.Headers.Add("Cookie", firstCookieHeader);
        var firstRefresh = await client.SendAsync(firstRefreshRequest);

        using var secondRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        secondRefreshRequest.Headers.Add("Cookie", secondCookieHeader);
        var secondRefresh = await client.SendAsync(secondRefreshRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, firstRefresh.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondRefresh.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithoutCookie_ReturnsUnauthorizedAndClearsCookie()
    {
        using var factory = new AuthApiFactory();
        var client = createClientWithoutCookies(factory);

        var response = await client.PostAsync("/auth/refresh", content: null);
        var refreshCookie = getRefreshCookie(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains($"{RefreshCookieName}=", refreshCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("expires=", refreshCookie, StringComparison.OrdinalIgnoreCase);
    }

    private static WebApplicationFactoryClientOptions noCookieOptions()
    {
        return new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        };
    }

    private static HttpClient createClientWithoutCookies(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(noCookieOptions());
    }

    private static async Task registerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name = "User",
            email = "user@example.com",
            password = "secret123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static Task<HttpResponseMessage> loginAsync(HttpClient client)
    {
        return client.PostAsJsonAsync("/auth/login", new
        {
            email = "user@example.com",
            password = "secret123"
        });
    }

    private static string getRefreshCookieHeader(HttpResponseMessage response)
    {
        var refreshCookie = getRefreshCookie(response);

        return refreshCookie.Split(';', 2)[0];
    }

    private static string getRefreshCookie(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));

        return Assert.Single(cookies, cookie => cookie.StartsWith($"{RefreshCookieName}=", StringComparison.Ordinal));
    }

    private static async Task<DateTime?> getLastLoginAsync(AuthApiFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync(user => user.Email == email);

        return user.LastLoginAtUtc;
    }

    private static async Task setLastLoginAsync(AuthApiFactory factory, string email, DateTime lastLoginAtUtc)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync(user => user.Email == email);

        user.LastLoginAtUtc = lastLoginAtUtc;
        await dbContext.SaveChangesAsync();
    }

    private sealed class AuthApiFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"auth-api-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            });
        }
    }
}
