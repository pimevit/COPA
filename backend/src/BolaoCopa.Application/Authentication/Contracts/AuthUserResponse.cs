namespace BolaoCopa.Application.Authentication.Contracts;

public sealed record AuthUserResponse(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt,
    IReadOnlyList<string> Roles);
