using BolaoCopa.Application.Authentication.Contracts;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Application.Authentication.Users;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Authentication;

public sealed class AuthService(
    IUserAuthRepository userRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    IUtcClock clock)
{
    public async Task<AuthResult<AuthTokenResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (!IsValidRegisterRequest(request, normalizedEmail, out var validationMessage))
        {
            return AuthResult<AuthTokenResponse>.Failure(AuthErrorCode.ValidationFailed, validationMessage);
        }

        var existingUser = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            return AuthResult<AuthTokenResponse>.Failure(AuthErrorCode.EmailAlreadyExists, "Email is already registered.");
        }

        var now = clock.UtcNow;
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            CreatedAt = now,
            LastLoginAtUtc = now
        };

        user.PasswordHash = passwordHashService.HashPassword(user, request.Password);

        await userRepository.CreateAsync(user, cancellationToken);

        var token = jwtTokenService.GenerateToken(user);

        return AuthResult<AuthTokenResponse>.Success(new AuthTokenResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            MapUser(user, token.Roles)));
    }

    public async Task<AuthResult<AuthTokenResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResult<AuthTokenResponse>.Failure(AuthErrorCode.InvalidCredentials, "Invalid email or password.");
        }

        var user = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !passwordHashService.VerifyPassword(user, request.Password, user.PasswordHash))
        {
            return AuthResult<AuthTokenResponse>.Failure(AuthErrorCode.InvalidCredentials, "Invalid email or password.");
        }

        user.LastLoginAtUtc = clock.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.GenerateToken(user);

        return AuthResult<AuthTokenResponse>.Success(new AuthTokenResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            MapUser(user, token.Roles)));
    }

    private static bool IsValidRegisterRequest(
        RegisterRequest request,
        string normalizedEmail,
        out string validationMessage)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            validationMessage = "Name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail) || !normalizedEmail.Contains('@', StringComparison.Ordinal))
        {
            validationMessage = "A valid email is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            validationMessage = "Password must contain at least 6 characters.";
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }

    private static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static AuthUserResponse MapUser(User user, IReadOnlyList<string> roles)
    {
        return new AuthUserResponse(user.Id, user.Name, user.Email, user.CreatedAt, roles);
    }
}
