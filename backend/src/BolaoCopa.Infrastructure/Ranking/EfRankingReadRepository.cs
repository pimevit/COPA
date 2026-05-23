using BolaoCopa.Application.Ranking.Data;
using BolaoCopa.Application.Ranking.ReadModels;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Ranking;

public sealed class EfRankingReadRepository(AppDbContext dbContext) : IRankingReadRepository
{
    public async Task<IReadOnlyList<RankingBetReadModel>> ListEvaluatedBetsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bets
            .AsNoTracking()
            .Where(bet => bet.Match != null
                && bet.User != null
                && bet.Match.HomeGoals.HasValue
                && bet.Match.AwayGoals.HasValue)
            .Select(bet => new RankingBetReadModel(
                bet.UserId,
                bet.User!.Name,
                bet.MatchId,
                bet.PointsEarned,
                bet.HomeGoalsPrediction,
                bet.AwayGoalsPrediction,
                bet.Match!.HomeGoals!.Value,
                bet.Match.AwayGoals!.Value,
                bet.Match.MatchDate,
                bet.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
