namespace BolaoCopa.Application.Authentication.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 30;
    public string[] AdminEmails { get; init; } = [];
}
