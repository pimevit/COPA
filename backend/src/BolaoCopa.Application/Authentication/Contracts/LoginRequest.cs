namespace BolaoCopa.Application.Authentication.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password);
