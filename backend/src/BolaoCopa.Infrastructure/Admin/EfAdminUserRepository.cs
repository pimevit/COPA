using BolaoCopa.Application.Admin.Users.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Admin;

public sealed class EfAdminUserRepository(AppDbContext dbContext) : IAdminUserRepository
{
    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Email)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> FindByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
