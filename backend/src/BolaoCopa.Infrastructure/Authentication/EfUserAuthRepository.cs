using BolaoCopa.Application.Authentication.Users;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Authentication;

public sealed class EfUserAuthRepository(AppDbContext dbContext) : IUserAuthRepository
{
    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
