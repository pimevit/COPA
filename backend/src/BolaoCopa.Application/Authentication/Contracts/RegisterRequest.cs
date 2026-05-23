namespace BolaoCopa.Application.Authentication.Contracts;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password);
