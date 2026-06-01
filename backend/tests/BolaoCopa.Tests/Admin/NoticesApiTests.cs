using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BolaoCopa.Tests.Admin;

public sealed class NoticesApiTests
{
    [Fact]
    public async Task GetMatchNotice_WhenEmpty_ReturnsEmptyMessage()
    {
        using var factory = new NoticesApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/notices/matches");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(string.Empty, document.RootElement.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("updatedAtUtc").ValueKind);
    }

    [Fact]
    public async Task PutMatchNotice_WhenAdmin_SavesAndReturnsPublicNotice()
    {
        using var factory = new NoticesApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com", "secret123");

        var saveResponse = await client.PutAsJsonAsync("/admin/notices/matches", new
        {
            message = "Rodada aberta ate sexta."
        });
        var publicResponse = await client.GetAsync("/notices/matches");
        using var document = JsonDocument.Parse(await publicResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        Assert.Equal("Rodada aberta ate sexta.", document.RootElement.GetProperty("message").GetString());
        Assert.NotEqual(JsonValueKind.Null, document.RootElement.GetProperty("updatedAtUtc").ValueKind);
    }

    [Fact]
    public async Task PutMatchNotice_WithEmptyMessage_WhenAdmin_ClearsNotice()
    {
        using var factory = new NoticesApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "admin@example.com", "secret123");

        await client.PutAsJsonAsync("/admin/notices/matches", new { message = "Aviso temporario" });
        var clearResponse = await client.PutAsJsonAsync("/admin/notices/matches", new { message = "" });
        var publicResponse = await client.GetAsync("/notices/matches");
        using var document = JsonDocument.Parse(await publicResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, clearResponse.StatusCode);
        Assert.Equal(string.Empty, document.RootElement.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("updatedAtUtc").ValueKind);
    }

    [Fact]
    public async Task PutMatchNotice_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        using var factory = new NoticesApiFactory();
        var client = factory.CreateClient();
        await authenticateAsync(client, "user@example.com", "secret123");

        var response = await client.PutAsJsonAsync("/admin/notices/matches", new
        {
            message = "Sem permissao"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

        var login = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password
        });
        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed class NoticesApiFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"notices-api-{Guid.NewGuid():N}";

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
