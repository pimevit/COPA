using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Application.Matches.ReadModels;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Matches;

public sealed class EfMatchReadRepository(AppDbContext dbContext) : IMatchReadRepository
{
    public async Task<IReadOnlyList<MatchReadModel>> ListAsync(
        MatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        var matches = createBaseQuery();

        if (query.Stage.HasValue)
        {
            matches = matches.Where(match => match.Stage == query.Stage.Value);
        }

        if (query.Status.HasValue)
        {
            matches = matches.Where(match => match.Status == query.Status.Value);
        }

        return await matches
            .OrderBy(match => match.MatchDate)
            .Select(match => new MatchReadModel(
                match.Id,
                new TeamReadModel(
                    match.HomeTeam!.Id,
                    match.HomeTeam.Name,
                    match.HomeTeam.Code,
                    match.HomeTeam.FlagUrl),
                new TeamReadModel(
                    match.AwayTeam!.Id,
                    match.AwayTeam.Name,
                    match.AwayTeam.Code,
                    match.AwayTeam.FlagUrl),
                match.MatchDate,
                match.Stage,
                match.Status,
                match.HomeGoals,
                match.AwayGoals,
                match.AllowBetUntil,
                match.IsBettingLocked))
            .ToListAsync(cancellationToken);
    }

    public Task<MatchReadModel?> FindByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return createBaseQuery()
            .Where(match => match.Id == id)
            .Select(match => new MatchReadModel(
                match.Id,
                new TeamReadModel(
                    match.HomeTeam!.Id,
                    match.HomeTeam.Name,
                    match.HomeTeam.Code,
                    match.HomeTeam.FlagUrl),
                new TeamReadModel(
                    match.AwayTeam!.Id,
                    match.AwayTeam.Name,
                    match.AwayTeam.Code,
                    match.AwayTeam.FlagUrl),
                match.MatchDate,
                match.Stage,
                match.Status,
                match.HomeGoals,
                match.AwayGoals,
                match.AllowBetUntil,
                match.IsBettingLocked))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<Domain.Entities.Match> createBaseQuery()
    {
        return dbContext.Matches.AsNoTracking();
    }
}
