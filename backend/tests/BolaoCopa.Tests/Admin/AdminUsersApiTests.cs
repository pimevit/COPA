using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BolaoCopa.Infrastructure.Persistence;
using Xunit;

namespace BolaoCopa.Tests.Admin;

public sealed class AdminUsersApiTests
{
    [Fact]
    public async Task GetAdminUsers_WhenAdmin_ReturnsUsersWithoutPasswordHash()
    {
        using var factory = new AdminUsersApiFactory();
        var client = factory.CreateClient();

        await registerAsync(client, "user@example.com", "secret123");
        await authenticateAsync(client, "admin@example.com", "secret123");

        var response = await client.GetAsync("/admin/users");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var users = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, users.Length);
        var adminUser = Assert.Single(users, user => user.GetProperty("email").GetString() == "admin@example.com");
        Assert.True(adminUser.TryGetProperty("lastLoginAtUtc", out var lastLoginAtUtc));
        Assert.NotEqual(JsonValueKind.Null, lastLoginAtUtc.ValueKind);
        Assert.All(users, user => Assert.False(user.TryGetProperty("passwordHash", out _)));
    }

    [Fact]
    public async Task GetAdminUsers_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new AdminUsersApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com", "secret123");

        var response = await client.GetAsync("/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenAdmin_GeneratesTemporaryPasswordAndReplacesLoginPassword()
    {
        using var factory = new AdminUsersApiFactory();
        var client = factory.CreateClient();
        var userId = await registerAsync(client, "user@example.com", "secret123");

        await authenticateAsync(client, "admin@example.com", "secret123");
        var response = await client.PostAsync($"/admin/users/{userId}/reset-password", content: null);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement;
        var temporaryPassword = body.GetProperty("temporaryPassword").GetString();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userId, body.GetProperty("userId").GetInt32());
        Assert.Equal("user@example.com", body.GetProperty("email").GetString());
        Assert.True(isComplexTemporaryPassword(temporaryPassword));

        var oldLogin = await loginAsync(client, "user@example.com", "secret123");
        var newLogin = await loginAsync(client, "user@example.com", temporaryPassword!);

        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenUserDoesNotExist_ReturnsNotFound()
    {
        using var factory = new AdminUsersApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com", "secret123");

        var response = await client.PostAsync("/admin/users/999/reset-password", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static bool isComplexTemporaryPassword(string? password)
    {
        return password is { Length: 8 }
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(character => !char.IsLetterOrDigit(character));
    }

    private static async Task<int> registerAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name = email.Split('@')[0],
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("user").GetProperty("id").GetInt32();
    }

    private static async Task authenticateAsync(HttpClient client, string email, string password)
    {
        var register = await client.PostAsJsonAsync("/auth/register", new
        {
            name = email.Split('@')[0],
            email,
            password
        });

        Assert.True(
            register.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict,
            $"Unexpected register status: {register.StatusCode}");

        var login = await loginAsync(client, email, password);
        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static Task<HttpResponseMessage> loginAsync(HttpClient client, string email, string password)
    {
        return client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password
        });
    }

    private sealed class AdminUsersApiFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"admin-users-api-{Guid.NewGuid():N}";

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
