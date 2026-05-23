namespace BolaoCopa.Application.Matches.ReadModels;

public sealed record TeamReadModel(
    int Id,
    string Name,
    string Code,
    string FlagUrl);
