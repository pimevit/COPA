using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using BolaoCopa.Api.Endpoints;
using BolaoCopa.Api.Middleware;
using BolaoCopa.Api.OpenApi;
using BolaoCopa.Api.Validation;
using BolaoCopa.Application.Admin.Users;
using BolaoCopa.Application.Authentication;
using BolaoCopa.Application.Authentication.Contracts;
using BolaoCopa.Application.Authentication.RefreshTokens;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Application.Bets;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Matches;
using BolaoCopa.Application.Notices;
using BolaoCopa.Application.Ranking;
using BolaoCopa.Application.Stats;
using BolaoCopa.Application.Teams;
using BolaoCopa.Domain.Scoring;
using BolaoCopa.Infrastructure;
using BolaoCopa.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);
var allowedCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedCorsOrigins is null || allowedCorsOrigins.Length == 0)
{
    allowedCorsOrigins = ["http://localhost:5173", "http://127.0.0.1:5173"];
}

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
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<AdminUsersService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<BetsService>();
builder.Services.AddScoped<MatchesService>();
builder.Services.AddScoped<MatchAdminService>();
builder.Services.AddScoped<MatchResultsService>();
builder.Services.AddScoped<MatchPointsRecalculator>();
builder.Services.AddScoped<NoticesService>();
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.IsRelational())
    {
        app.Logger.LogInformation("Applying database migrations.");
        await dbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied.");
    }
    else
    {
        app.Logger.LogInformation("Skipping database migrations because the configured provider is not relational.");
    }
}

/*if (app.Environment.IsDevelopment())
{*/
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

var routes = app.MapGroup(string.Empty)
    .AddEndpointFilter<ValidationFilter>();

// GET /health - confirma que a API está respondendo.
routes.MapGet("/health", () => Results.Ok());
routes.MapAuthEndpoints();
routes.MapBetsEndpoints();
routes.MapMatchesEndpoints();
routes.MapNoticesEndpoints();
routes.MapRankingEndpoints();
routes.MapStatsEndpoints();
routes.MapTeamsEndpoints();
routes.MapAdminMaintenanceEndpoints();
routes.MapAdminUsersEndpoints();

app.Run();

public partial class Program;
