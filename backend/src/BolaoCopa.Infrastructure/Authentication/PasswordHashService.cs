using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BolaoCopa.Infrastructure.Authentication;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<User> passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password, string passwordHash)
    {
        var result = passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
