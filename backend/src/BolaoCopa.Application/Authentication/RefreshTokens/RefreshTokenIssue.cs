namespace BolaoCopa.Application.Authentication.RefreshTokens;

public sealed record RefreshTokenIssue(
    string Token,
    DateTime ExpiresAtUtc);
