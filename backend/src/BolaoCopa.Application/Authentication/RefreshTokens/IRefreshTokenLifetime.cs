namespace BolaoCopa.Application.Authentication.RefreshTokens;

public interface IRefreshTokenLifetime
{
    TimeSpan Lifetime { get; }
}
