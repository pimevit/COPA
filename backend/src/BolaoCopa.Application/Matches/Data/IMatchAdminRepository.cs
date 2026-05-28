using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Application.Matches.Data;

public interface IMatchAdminRepository
{
    Task<Team?> FindTeamAsync(int teamId, CancellationToken cancellationToken = default);
    Task<Match?> FindMatchAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Match?> FindMatchForDeletionAsync(int matchId, CancellationToken cancellationToken = default);
    Task<bool> MatchExistsAsync(
        int homeTeamId,
        int awayTeamId,
        DateTime matchDate,
        Stage stage,
        CancellationToken cancellationToken = default);
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
    Task DeleteBetsAsync(IEnumerable<Bet> bets, CancellationToken cancellationToken = default);
    Task DeleteAsync(Match match, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
