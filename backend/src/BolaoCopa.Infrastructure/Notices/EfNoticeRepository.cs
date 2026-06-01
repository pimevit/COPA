using BolaoCopa.Application.Notices.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Notices;

public sealed class EfNoticeRepository(AppDbContext dbContext) : INoticeRepository
{
    public Task<Notice?> FindByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return dbContext.Notices.SingleOrDefaultAsync(notice => notice.Key == key, cancellationToken);
    }

    public async Task AddAsync(Notice notice, CancellationToken cancellationToken = default)
    {
        await dbContext.Notices.AddAsync(notice, cancellationToken);
    }

    public void Remove(Notice notice)
    {
        dbContext.Notices.Remove(notice);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
