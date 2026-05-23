using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Application.Matches.ReadModels;

namespace BolaoCopa.Application.Matches;

public sealed class MatchesService(
    IMatchReadRepository matchReadRepository,
    IUtcClock utcClock)
{
    public async Task<IReadOnlyList<MatchListItemResponse>> ListAsync(
        MatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        var matches = await matchReadRepository.ListAsync(query, cancellationToken);
        var nowUtc = utcClock.UtcNow;

        return matches
            .Select(match => new MatchListItemResponse(
                match.Id,
                mapTeam(match.HomeTeam),
                mapTeam(match.AwayTeam),
                match.MatchDate,
                match.Stage.ToString(),
                match.Status.ToString(),
                match.HomeGoals,
                match.AwayGoals,
                BettingWindow.IsBettingOpen(match.AllowBetUntil, nowUtc)))
            .ToList();
    }

    public async Task<MatchDetailResponse?> FindByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var match = await matchReadRepository.FindByIdAsync(id, cancellationToken);
        if (match is null)
        {
            return null;
        }

        var nowUtc = utcClock.UtcNow;

        return new MatchDetailResponse(
            match.Id,
            mapTeam(match.HomeTeam),
            mapTeam(match.AwayTeam),
            match.MatchDate,
            match.Stage.ToString(),
            match.Status.ToString(),
            match.HomeGoals,
            match.AwayGoals,
            BettingWindow.IsBettingOpen(match.AllowBetUntil, nowUtc));
    }

    private static TeamSummaryResponse mapTeam(TeamReadModel team)
    {
        return new TeamSummaryResponse(team.Id, team.Name, team.Code, team.FlagUrl);
    }
}
