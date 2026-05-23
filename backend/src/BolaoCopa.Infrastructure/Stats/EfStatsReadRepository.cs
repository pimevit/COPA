using BolaoCopa.Application.Stats.Data;
using BolaoCopa.Application.Stats.ReadModels;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Stats;

public sealed class EfStatsReadRepository(AppDbContext dbContext) : IStatsReadRepository
{
    public async Task<IReadOnlyList<StatsBetReadModel>> ListEvaluatedBetsByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bets
            .AsNoTracking()
            .Where(bet => bet.UserId == userId
                && bet.Match != null
                && bet.Match.HomeGoals.HasValue
                && bet.Match.AwayGoals.HasValue)
            .Select(bet => new StatsBetReadModel(
                bet.MatchId,
                bet.PointsEarned,
                bet.HomeGoalsPrediction,
                bet.AwayGoalsPrediction,
                bet.Match!.HomeGoals!.Value,
                bet.Match.AwayGoals!.Value,
                bet.Match.MatchDate))
            .ToListAsync(cancellationToken);
    }
}
