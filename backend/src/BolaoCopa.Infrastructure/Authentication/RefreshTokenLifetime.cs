using BolaoCopa.Application.Authentication.RefreshTokens;
using BolaoCopa.Application.Authentication.Security;
using Microsoft.Extensions.Options;

namespace BolaoCopa.Infrastructure.Authentication;

public sealed class RefreshTokenLifetime(IOptions<JwtOptions> options) : IRefreshTokenLifetime
{
    public TimeSpan Lifetime => TimeSpan.FromDays(options.Value.RefreshTokenExpirationDays);
}
