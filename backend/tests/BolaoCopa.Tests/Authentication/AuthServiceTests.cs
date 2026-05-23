using BolaoCopa.Application.Authentication;
using BolaoCopa.Application.Authentication.Contracts;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Application.Authentication.Users;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Authentication;
using Xunit;

namespace BolaoCopa.Tests.Authentication;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserWithHashedPassword()
    {
        var repository = new FakeUserAuthRepository();
        var service = CreateService(repository);

        var result = await service.RegisterAsync(new RegisterRequest(
            "Felipe",
            "FELIPE@example.com",
            "secret123"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);

        var user = Assert.Single(repository.Users);
        Assert.Equal("felipe@example.com", user.Email);
        Assert.NotEqual("secret123", user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);
        Assert.Equal("test-token", result.Value.AccessToken);
        Assert.Equal(user.Email, result.Value.User.Email);
        Assert.Empty(result.Value.User.Roles);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFailureWhenEmailAlreadyExists()
    {
        var repository = new FakeUserAuthRepository();
        var existingUser = new User { Name = "Felipe", Email = "felipe@example.com" };
        existingUser.PasswordHash = new PasswordHashService().HashPassword(existingUser, "secret123");
        repository.Users.Add(existingUser);

        var service = CreateService(repository);

        var result = await service.RegisterAsync(new RegisterRequest(
            "Other",
            "FELIPE@example.com",
            "secret123"));

        Assert.False(result.Succeeded);
        Assert.Equal(AuthErrorCode.EmailAlreadyExists, result.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenWhenCredentialsAreValid()
    {
        var repository = new FakeUserAuthRepository();
        var passwordHashService = new PasswordHashService();
        var user = new User { Id = 7, Name = "Felipe", Email = "felipe@example.com" };
        user.PasswordHash = passwordHashService.HashPassword(user, "secret123");
        repository.Users.Add(user);

        var service = new AuthService(repository, passwordHashService, new FakeJwtTokenService());

        var result = await service.LoginAsync(new LoginRequest("FELIPE@example.com", "secret123"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("test-token", result.Value.AccessToken);
        Assert.Equal(user.Id, result.Value.User.Id);
        Assert.Empty(result.Value.User.Roles);
    }

    [Fact]
    public async Task LoginAsync_WhenTokenHasAdminRole_ReturnsUserRoles()
    {
        var repository = new FakeUserAuthRepository();
        var passwordHashService = new PasswordHashService();
        var user = new User { Id = 7, Name = "Admin", Email = "admin@example.com" };
        user.PasswordHash = passwordHashService.HashPassword(user, "secret123");
        repository.Users.Add(user);

        var service = new AuthService(repository, passwordHashService, new FakeJwtTokenService());

        var result = await service.LoginAsync(new LoginRequest("admin@example.com", "secret123"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(["Admin"], result.Value.User.Roles);
    }

    [Fact]
    public async Task LoginAsync_ReturnsSingleFailureForInvalidCredentials()
    {
        var service = CreateService(new FakeUserAuthRepository());

        var result = await service.LoginAsync(new LoginRequest("missing@example.com", "wrong123"));

        Assert.False(result.Succeeded);
        Assert.Equal(AuthErrorCode.InvalidCredentials, result.ErrorCode);
        Assert.Equal("Invalid email or password.", result.ErrorMessage);
    }

    private static AuthService CreateService(FakeUserAuthRepository repository)
    {
        return new AuthService(repository, new PasswordHashService(), new FakeJwtTokenService());
    }

    private sealed class FakeUserAuthRepository : IUserAuthRepository
    {
        public List<User> Users { get; } = [];

        public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            var user = Users.SingleOrDefault(user => user.Email == normalizedEmail);
            return Task.FromResult(user);
        }

        public Task CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            user.Id = Users.Count + 1;
            Users.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public JwtToken GenerateToken(User user)
        {
            var roles = user.Email == "admin@example.com"
                ? new[] { "Admin" }
                : [];

            return new JwtToken("test-token", DateTime.UtcNow.AddMinutes(30), roles);
        }
    }
}
