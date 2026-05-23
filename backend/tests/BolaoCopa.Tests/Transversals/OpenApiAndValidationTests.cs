using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BolaoCopa.Tests.Transversals;

public sealed class OpenApiAndValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public OpenApiAndValidationTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
    }

    [Fact]
    public async Task SwaggerJson_IncludesEndpointsAndBearerSecurity()
    {
        var client = factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");

        Assert.Contains("\"/health\"", json);
        Assert.Contains("\"/auth/login\"", json);
        Assert.Contains("\"Bearer\"", json);
        Assert.Contains("\"bearer\"", json);
    }

    [Fact]
    public async Task InvalidRequest_ReturnsValidationProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name = "",
            email = "invalid-email",
            password = "123"
        });

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.True(root.TryGetProperty("traceId", out _));
        Assert.True(root.GetProperty("errors").TryGetProperty("Name", out _));
        Assert.True(root.GetProperty("errors").TryGetProperty("Email", out _));
        Assert.True(root.GetProperty("errors").TryGetProperty("Password", out _));
    }

    [Fact]
    public async Task LocalFrontendPreflight_ReturnsCorsHeaders()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/auth/register");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Contains("http://localhost:5173", origins);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Methods", out var methods));
        Assert.Contains("POST", string.Join(",", methods));
    }
}
