using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BolaoCopa.Application.Authentication.Security;
using BolaoCopa.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BolaoCopa.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public JwtToken GenerateToken(User user)
    {
        var jwtOptions = options.Value;
        if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT secret was not configured.");
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var roles = new List<string>();
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        if (isAdmin(user.Email, jwtOptions.AdminEmails))
        {
            roles.Add("Admin");
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc, roles);
    }

    private static bool isAdmin(string email, IEnumerable<string> adminEmails)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return adminEmails
            .Select(adminEmail => adminEmail.Trim().ToLowerInvariant())
            .Contains(normalizedEmail, StringComparer.Ordinal);
    }
}
