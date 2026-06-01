namespace BolaoCopa.Application.Authentication.RefreshTokens;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
    string HashToken(string token);
}
