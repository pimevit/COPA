namespace BolaoCopa.Application.Admin.Users;

public sealed record AdminUserResponse(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt);
