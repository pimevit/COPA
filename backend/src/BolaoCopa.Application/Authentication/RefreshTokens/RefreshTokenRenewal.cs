using BolaoCopa.Application.Authentication.Contracts;

namespace BolaoCopa.Application.Authentication.RefreshTokens;

public sealed record RefreshTokenRenewal(
    AuthTokenResponse Session,
    RefreshTokenIssue RefreshToken);
