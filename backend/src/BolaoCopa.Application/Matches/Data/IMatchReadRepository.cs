using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.ReadModels;

namespace BolaoCopa.Application.Matches.Data;

public interface IMatchReadRepository
{
    Task<IReadOnlyList<MatchReadModel>> ListAsync(
        MatchesQuery query,
        CancellationToken cancellationToken = default);

    Task<MatchReadModel?> FindByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}
