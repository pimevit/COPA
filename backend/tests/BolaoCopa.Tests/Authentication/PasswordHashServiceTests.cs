using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Authentication;
using Xunit;

namespace BolaoCopa.Tests.Authentication;

public sealed class PasswordHashServiceTests
{
    [Fact]
    public void VerifyPassword_ReturnsTrueForMatchingPassword()
    {
        var user = new User { Id = 1, Email = "felipe@example.com" };
        var service = new PasswordHashService();
        var hash = service.HashPassword(user, "secret123");

        var isValid = service.VerifyPassword(user, "secret123", hash);

        Assert.True(isValid);
        Assert.NotEqual("secret123", hash);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForDifferentPassword()
    {
        var user = new User { Id = 1, Email = "felipe@example.com" };
        var service = new PasswordHashService();
        var hash = service.HashPassword(user, "secret123");

        var isValid = service.VerifyPassword(user, "wrong123", hash);

        Assert.False(isValid);
    }
}
