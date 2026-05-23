using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Authentication.Security;

public interface IPasswordHashService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}
