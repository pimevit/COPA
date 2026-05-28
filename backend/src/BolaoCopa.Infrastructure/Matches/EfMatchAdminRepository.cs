using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Matches;

public sealed class EfMatchAdminRepository(AppDbContext dbContext) : IMatchAdminRepository
{
    public Task<Team?> FindTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        return dbContext.Teams.SingleOrDefaultAsync(team => team.Id == teamId, cancellationToken);
    }

    public Task<Match?> FindMatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return dbContext.Matches.SingleOrDefaultAsync(match => match.Id == matchId, cancellationToken);
    }

    public Task<Match?> FindMatchForDeletionAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return dbContext.Matches
            .Include(match => match.Bets)
            .SingleOrDefaultAsync(match => match.Id == matchId, cancellationToken);
    }

    public Task<bool> MatchExistsAsync(
        int homeTeamId,
        int awayTeamId,
        DateTime matchDate,
        Stage stage,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Matches.AnyAsync(match =>
            match.HomeTeamId == homeTeamId
            && match.AwayTeamId == awayTeamId
            && match.MatchDate == matchDate
            && match.Stage == stage,
            cancellationToken);
    }

    public Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        dbContext.Matches.Add(match);
        return Task.CompletedTask;
    }

    public Task DeleteBetsAsync(IEnumerable<Bet> bets, CancellationToken cancellationToken = default)
    {
        dbContext.Bets.RemoveRange(bets);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Match match, CancellationToken cancellationToken = default)
    {
        dbContext.Matches.Remove(match);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
