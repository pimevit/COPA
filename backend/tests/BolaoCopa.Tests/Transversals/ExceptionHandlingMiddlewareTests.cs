using System.Net;
using BolaoCopa.Api.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BolaoCopa.Tests.Transversals;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReturnsProblemDetailsWithoutStackTraceInProduction()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        httpContext.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("database password failed at secret path"),
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            new FakeHostEnvironment { EnvironmentName = Environments.Production });

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var body = await reader.ReadToEndAsync();

        Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);
        Assert.Contains("\"status\":500", body);
        Assert.Contains("\"traceId\"", body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("database password failed", body);
        Assert.DoesNotContain(" at ", body);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "BolaoCopa.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
