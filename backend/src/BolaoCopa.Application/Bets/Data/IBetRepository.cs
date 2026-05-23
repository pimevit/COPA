using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Bets.Data;

public interface IBetRepository
{
    Task<Match?> FindMatchByIdAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Bet?> FindByUserAndMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task<Bet?> FindByIdAndUserAsync(int betId, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bet>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(Bet bet, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
