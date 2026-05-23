using BolaoCopa.Application.Ranking.ReadModels;

namespace BolaoCopa.Application.Ranking.Data;

public interface IRankingReadRepository
{
    Task<IReadOnlyList<RankingBetReadModel>> ListEvaluatedBetsAsync(
        CancellationToken cancellationToken = default);
}
