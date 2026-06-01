using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Notices.Contracts;
using BolaoCopa.Application.Notices.Data;
using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Notices;

public sealed class NoticesService(INoticeRepository noticeRepository, IUtcClock clock)
{
    public const int MaxMessageLength = 500;
    private const string MatchesNoticeKey = "matches";

    public async Task<NoticeResponse> GetMatchesNoticeAsync(CancellationToken cancellationToken = default)
    {
        var notice = await noticeRepository.FindByKeyAsync(MatchesNoticeKey, cancellationToken);

        return mapNotice(notice);
    }

    public async Task<NoticeResponse> UpdateMatchesNoticeAsync(
        string? message,
        CancellationToken cancellationToken = default)
    {
        var normalizedMessage = message?.Trim() ?? string.Empty;
        var notice = await noticeRepository.FindByKeyAsync(MatchesNoticeKey, cancellationToken);

        if (normalizedMessage.Length == 0)
        {
            if (notice is not null)
            {
                noticeRepository.Remove(notice);
                await noticeRepository.SaveChangesAsync(cancellationToken);
            }

            return new NoticeResponse(string.Empty, null);
        }

        var updatedAtUtc = clock.UtcNow;

        if (notice is null)
        {
            notice = new Notice
            {
                Key = MatchesNoticeKey,
                Message = normalizedMessage,
                UpdatedAtUtc = updatedAtUtc
            };

            await noticeRepository.AddAsync(notice, cancellationToken);
        }
        else
        {
            notice.Message = normalizedMessage;
            notice.UpdatedAtUtc = updatedAtUtc;
        }

        await noticeRepository.SaveChangesAsync(cancellationToken);

        return mapNotice(notice);
    }

    private static NoticeResponse mapNotice(Notice? notice)
    {
        return notice is null
            ? new NoticeResponse(string.Empty, null)
            : new NoticeResponse(notice.Message, notice.UpdatedAtUtc);
    }
}
