using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Authentication.Users;

public interface IUserAuthRepository
{
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task CreateAsync(User user, CancellationToken cancellationToken = default);
}
