using BolaoCopa.Application.Stats.ReadModels;

namespace BolaoCopa.Application.Stats.Data;

public interface IStatsReadRepository
{
    Task<IReadOnlyList<StatsBetReadModel>> ListEvaluatedBetsByUserAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
