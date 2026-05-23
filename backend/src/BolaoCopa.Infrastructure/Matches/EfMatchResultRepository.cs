using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Matches;

public sealed class EfMatchResultRepository(AppDbContext dbContext) : IMatchResultRepository
{
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var result = await operation(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    public Task<Match?> FindMatchWithBetsAsync(
        int matchId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Matches
            .Include(match => match.Bets)
            .SingleOrDefaultAsync(match => match.Id == matchId, cancellationToken);
    }
}
