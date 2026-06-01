namespace BolaoCopa.Application.Notices.Contracts;

public sealed record NoticeResponse(
    string Message,
    DateTime? UpdatedAtUtc);
