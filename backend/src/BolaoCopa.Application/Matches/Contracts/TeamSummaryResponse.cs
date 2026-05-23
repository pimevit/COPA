namespace BolaoCopa.Application.Matches.Contracts;

public sealed record TeamSummaryResponse(
    int Id,
    string Name,
    string Code,
    string FlagUrl);
