using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Notices.Data;

public interface INoticeRepository
{
    Task<Notice?> FindByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task AddAsync(Notice notice, CancellationToken cancellationToken = default);
    void Remove(Notice notice);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
