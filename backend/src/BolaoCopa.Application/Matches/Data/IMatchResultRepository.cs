using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Matches.Data;

public interface IMatchResultRepository
{
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    Task<Match?> FindMatchWithBetsAsync(int matchId, CancellationToken cancellationToken = default);
}
