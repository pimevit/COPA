using BolaoCopa.Application.Teams.Contracts;

namespace BolaoCopa.Application.Teams.Data;

public interface ITeamReadRepository
{
    Task<IReadOnlyList<TeamResponse>> ListAsync(CancellationToken cancellationToken = default);
}
