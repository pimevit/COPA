using BolaoCopa.Application.Bets.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Bets;

public sealed class EfBetRepository(AppDbContext dbContext) : IBetRepository
{
    public Task<Match?> FindMatchByIdAsync(
        int matchId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Matches
            .Include(match => match.HomeTeam)
            .Include(match => match.AwayTeam)
            .SingleOrDefaultAsync(match => match.Id == matchId, cancellationToken);
    }

    public Task<User?> FindUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<Bet?> FindByUserAndMatchAsync(
        int userId,
        int matchId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Bets
            .SingleOrDefaultAsync(
                bet => bet.UserId == userId && bet.MatchId == matchId,
                cancellationToken);
    }

    public Task<Bet?> FindByIdAndUserAsync(
        int betId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Bets
            .Include(bet => bet.Match)
                .ThenInclude(match => match!.HomeTeam)
            .Include(bet => bet.Match)
                .ThenInclude(match => match!.AwayTeam)
            .SingleOrDefaultAsync(
                bet => bet.Id == betId && bet.UserId == userId,
                cancellationToken);
    }

    public Task<bool> MatchExistsAsync(
        int matchId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Matches
            .AsNoTracking()
            .AnyAsync(match => match.Id == matchId, cancellationToken);
    }

    public async Task<IReadOnlyList<Bet>> ListByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bets
            .AsNoTracking()
            .Include(bet => bet.Match)
                .ThenInclude(match => match!.HomeTeam)
            .Include(bet => bet.Match)
                .ThenInclude(match => match!.AwayTeam)
            .Where(bet => bet.UserId == userId)
            .OrderByDescending(bet => bet.Match!.MatchDate)
            .ThenByDescending(bet => bet.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bet>> ListPublicAsync(
        int? matchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Bets
            .AsNoTracking()
            .Include(bet => bet.User)
            .Where(bet => bet.User != null && bet.User.ShowBetsPublicly);

        if (matchId.HasValue)
        {
            query = query.Where(bet => bet.MatchId == matchId.Value);
        }

        return await query
            .OrderBy(bet => bet.MatchId)
            .ThenBy(bet => bet.User!.Name)
            .ThenBy(bet => bet.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Bet bet, CancellationToken cancellationToken = default)
    {
        await dbContext.Bets.AddAsync(bet, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
