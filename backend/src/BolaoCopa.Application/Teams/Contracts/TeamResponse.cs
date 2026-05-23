namespace BolaoCopa.Application.Teams.Contracts;

public sealed record TeamResponse(
    int Id,
    string Name,
    string Code,
    string FlagUrl);
