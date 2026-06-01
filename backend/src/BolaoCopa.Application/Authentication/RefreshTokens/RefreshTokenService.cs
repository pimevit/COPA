using BolaoCopa.Application.Authentication.Contracts;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Authentication.RefreshTokens;

public sealed class RefreshTokenService(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IJwtTokenService jwtTokenService,
    IRefreshTokenLifetime refreshTokenLifetime,
    IUtcClock clock)
{
    public async Task<RefreshTokenIssue> CreateForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = issueRefreshToken(userId);

        await refreshTokenRepository.AddAsync(refreshToken.Entity, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return refreshToken.Issue;
    }

    public async Task<AuthResult<RefreshTokenRenewal>> RefreshAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return AuthResult<RefreshTokenRenewal>.Failure(
                AuthErrorCode.InvalidCredentials,
                "Refresh token is missing.");
        }

        var tokenHash = refreshTokenGenerator.HashToken(refreshToken);
        var storedRefreshToken = await refreshTokenRepository.FindByHashAsync(tokenHash, cancellationToken);
        if (storedRefreshToken is null ||
            storedRefreshToken.User is null ||
            storedRefreshToken.RevokedAtUtc is not null ||
            storedRefreshToken.ExpiresAtUtc <= clock.UtcNow)
        {
            return AuthResult<RefreshTokenRenewal>.Failure(
                AuthErrorCode.InvalidCredentials,
                "Refresh token is invalid.");
        }

        var now = clock.UtcNow;
        storedRefreshToken.RevokedAtUtc = now;
        storedRefreshToken.User.LastLoginAtUtc = now;

        var newRefreshToken = issueRefreshToken(storedRefreshToken.UserId);
        await refreshTokenRepository.AddAsync(newRefreshToken.Entity, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateToken(storedRefreshToken.User);
        var response = new AuthTokenResponse(
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            mapUser(storedRefreshToken.User, accessToken.Roles));

        return AuthResult<RefreshTokenRenewal>.Success(new RefreshTokenRenewal(response, newRefreshToken.Issue));
    }

    public async Task RevokeAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = refreshTokenGenerator.HashToken(refreshToken);
        var storedRefreshToken = await refreshTokenRepository.FindByHashAsync(tokenHash, cancellationToken);
        if (storedRefreshToken is null || storedRefreshToken.RevokedAtUtc is not null)
        {
            return;
        }

        storedRefreshToken.RevokedAtUtc = clock.UtcNow;
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private (RefreshToken Entity, RefreshTokenIssue Issue) issueRefreshToken(int userId)
    {
        var token = refreshTokenGenerator.GenerateToken();
        var expiresAtUtc = clock.UtcNow.Add(refreshTokenLifetime.Lifetime);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = refreshTokenGenerator.HashToken(token),
            CreatedAtUtc = clock.UtcNow,
            ExpiresAtUtc = expiresAtUtc
        };

        return (refreshToken, new RefreshTokenIssue(token, expiresAtUtc));
    }

    private static AuthUserResponse mapUser(User user, IReadOnlyList<string> roles)
    {
        return new AuthUserResponse(user.Id, user.Name, user.Email, user.CreatedAt, roles);
    }
}
