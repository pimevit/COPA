namespace BolaoCopa.Application.Authentication.Security;

public sealed record JwtToken(
    string AccessToken,
    DateTime ExpiresAtUtc,
    IReadOnlyList<string> Roles);
