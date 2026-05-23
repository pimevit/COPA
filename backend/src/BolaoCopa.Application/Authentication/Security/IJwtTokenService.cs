using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Authentication.Security;

public interface IJwtTokenService
{
    JwtToken GenerateToken(User user);
}
