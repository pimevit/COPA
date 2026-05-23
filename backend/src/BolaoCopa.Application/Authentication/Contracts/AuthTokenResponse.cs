namespace BolaoCopa.Application.Authentication.Contracts;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    AuthUserResponse User);
