using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Application.Authentication.Users;
using BolaoCopa.Application.Bets.Data;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Application.Ranking.Data;
using BolaoCopa.Application.Stats.Data;
using BolaoCopa.Application.Teams.Data;
using BolaoCopa.Infrastructure.Authentication;
using BolaoCopa.Infrastructure.Bets;
using BolaoCopa.Infrastructure.Matches;
using BolaoCopa.Infrastructure.Persistence;
using BolaoCopa.Infrastructure.Persistence.Seeding;
using BolaoCopa.Infrastructure.Ranking;
using BolaoCopa.Infrastructure.Stats;
using BolaoCopa.Infrastructure.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BolaoCopa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<DatabaseSeedRunner>();
        services.AddScoped<IUserAuthRepository, EfUserAuthRepository>();
        services.AddScoped<IBetRepository, EfBetRepository>();
        services.AddScoped<IMatchReadRepository, EfMatchReadRepository>();
        services.AddScoped<IMatchAdminRepository, EfMatchAdminRepository>();
        services.AddScoped<IMatchResultRepository, EfMatchResultRepository>();
        services.AddScoped<IRankingReadRepository, EfRankingReadRepository>();
        services.AddScoped<IStatsReadRepository, EfStatsReadRepository>();
        services.AddScoped<ITeamReadRepository, EfTeamReadRepository>();
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
