namespace BolaoCopa.Application.Admin.Users;

public sealed record ResetUserPasswordResponse(
    int UserId,
    string Name,
    string Email,
    string TemporaryPassword);
