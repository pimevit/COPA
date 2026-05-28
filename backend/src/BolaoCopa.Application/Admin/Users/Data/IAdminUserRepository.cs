using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Admin.Users.Data;

public interface IAdminUserRepository
{
    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default);
    Task<User?> FindByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
