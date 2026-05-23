using BolaoCopa.Application.Teams.Contracts;
using BolaoCopa.Application.Teams.Data;

namespace BolaoCopa.Application.Teams;

public sealed class TeamsService(ITeamReadRepository teamReadRepository)
{
    public Task<IReadOnlyList<TeamResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return teamReadRepository.ListAsync(cancellationToken);
    }
}
