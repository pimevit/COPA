using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using BolaoCopa.Api.Endpoints;
using BolaoCopa.Api.Middleware;
using BolaoCopa.Api.OpenApi;
using BolaoCopa.Api.Validation;
using BolaoCopa.Application.Authentication;
using BolaoCopa.Application.Authentication.Contracts;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Application.Bets;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Matches;
using BolaoCopa.Application.Ranking;
using BolaoCopa.Application.Stats;
using BolaoCopa.Application.Teams;
using BolaoCopa.Domain.Scoring;
using BolaoCopa.Infrastructure;
using BolaoCopa.Infrastructure.Persistence.Seeding;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string LocalFrontendCorsPolicy = "LocalFrontend";

var builder = WebApplication.CreateBuilder(args);

ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("en");

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions =
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.SpanId |
        ActivityTrackingOptions.ParentId;
});
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

builder.Services.AddOpenApiDocumentation();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequest>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalFrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BetsService>();
builder.Services.AddScoped<MatchesService>();
builder.Services.AddScoped<MatchAdminService>();
builder.Services.AddScoped<MatchResultsService>();
builder.Services.AddScoped<MatchPointsRecalculator>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<TeamsService>();
builder.Services.AddSingleton<ScoreCalculator>();
builder.Services.AddSingleton<IUtcClock, SystemUtcClock>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seedRunner = scope.ServiceProvider.GetRequiredService<DatabaseSeedRunner>();

    await seedRunner.SeedAsync();
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors(LocalFrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

var routes = app.MapGroup(string.Empty)
    .AddEndpointFilter<ValidationFilter>();

// GET /health - confirma que a API está respondendo.
routes.MapGet("/health", () => Results.Ok());
routes.MapAuthEndpoints();
routes.MapBetsEndpoints();
routes.MapMatchesEndpoints();
routes.MapRankingEndpoints();
routes.MapStatsEndpoints();
routes.MapTeamsEndpoints();

app.Run();

public partial class Program;
