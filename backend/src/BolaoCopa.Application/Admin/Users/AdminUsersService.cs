using System.Security.Cryptography;
using BolaoCopa.Application.Admin.Users.Data;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Admin.Users;

public sealed class AdminUsersService(
    IAdminUserRepository userRepository,
    IPasswordHashService passwordHashService)
{
    private const int TemporaryPasswordLength = 8;
    private const string UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijkmnopqrstuvwxyz";
    private const string NumberChars = "23456789";
    private const string SymbolChars = "!@#$%&*?";
    private const string AllChars = UppercaseChars + LowercaseChars + NumberChars + SymbolChars;

    public async Task<IReadOnlyList<AdminUserResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.ListAsync(cancellationToken);

        return users
            .Select(mapUser)
            .ToList();
    }

    public async Task<ResetUserPasswordResponse?> ResetPasswordAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var temporaryPassword = generateTemporaryPassword();
        user.PasswordHash = passwordHashService.HashPassword(user, temporaryPassword);

        await userRepository.SaveChangesAsync(cancellationToken);

        return new ResetUserPasswordResponse(
            user.Id,
            user.Name,
            user.Email,
            temporaryPassword);
    }

    private static AdminUserResponse mapUser(User user)
    {
        return new AdminUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt);
    }

    private static string generateTemporaryPassword()
    {
        var chars = new char[TemporaryPasswordLength];
        chars[0] = pickChar(UppercaseChars);
        chars[1] = pickChar(LowercaseChars);
        chars[2] = pickChar(NumberChars);
        chars[3] = pickChar(SymbolChars);

        for (var index = 4; index < chars.Length; index++)
        {
            chars[index] = pickChar(AllChars);
        }

        shuffle(chars);
        return new string(chars);
    }

    private static char pickChar(string source)
    {
        return source[RandomNumberGenerator.GetInt32(source.Length)];
    }

    private static void shuffle(char[] chars)
    {
        for (var index = chars.Length - 1; index > 0; index--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (chars[index], chars[swapIndex]) = (chars[swapIndex], chars[index]);
        }
    }
}
