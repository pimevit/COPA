using BolaoCopa.Application.Teams.Contracts;
using BolaoCopa.Application.Teams.Data;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Teams;

public sealed class EfTeamReadRepository(AppDbContext dbContext) : ITeamReadRepository
{
    public async Task<IReadOnlyList<TeamResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Teams
            .AsNoTracking()
            .OrderBy(team => team.Name)
            .Select(team => new TeamResponse(
                team.Id,
                team.Name,
                team.Code,
                team.FlagUrl))
            .ToListAsync(cancellationToken);
    }
}
