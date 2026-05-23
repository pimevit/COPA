using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Application.Matches.Data;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Application.Matches;

public sealed class MatchAdminService(
    IMatchAdminRepository matchAdminRepository,
    IUtcClock utcClock)
{
    public async Task<MatchResult<MatchDetailResponse>> CreateAsync(
        CreateMatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var homeTeamId = request.HomeTeamId!.Value;
        var awayTeamId = request.AwayTeamId!.Value;
        var matchDate = ensureUtc(request.MatchDate!.Value);
        var stage = request.Stage!.Value;

        if (homeTeamId == awayTeamId)
        {
            return MatchResult<MatchDetailResponse>.Failure(
                MatchResultErrorCode.SameTeams,
                "Home team and away team must be different.");
        }

        var homeTeam = await matchAdminRepository.FindTeamAsync(homeTeamId, cancellationToken);
        var awayTeam = await matchAdminRepository.FindTeamAsync(awayTeamId, cancellationToken);
        if (homeTeam is null || awayTeam is null)
        {
            return MatchResult<MatchDetailResponse>.Failure(
                MatchResultErrorCode.TeamNotFound,
                "One or more teams were not found.");
        }

        if (await matchAdminRepository.MatchExistsAsync(homeTeamId, awayTeamId, matchDate, stage, cancellationToken))
        {
            return MatchResult<MatchDetailResponse>.Failure(
                MatchResultErrorCode.DuplicateMatch,
                "Match already exists for the selected teams, date and stage.");
        }

        var match = new Match
        {
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            HomeGoals = null,
            AwayGoals = null,
            MatchDate = matchDate,
            Stage = stage,
            Status = MatchStatus.Scheduled,
            AllowBetUntil = BettingWindow.CalculateAllowBetUntil(matchDate, stage),
            HomeTeam = homeTeam,
            AwayTeam = awayTeam
        };

        await matchAdminRepository.AddAsync(match, cancellationToken);
        await matchAdminRepository.SaveChangesAsync(cancellationToken);

        return MatchResult<MatchDetailResponse>.Success(new MatchDetailResponse(
            match.Id,
            mapTeam(homeTeam),
            mapTeam(awayTeam),
            match.MatchDate,
            match.Stage.ToString(),
            match.Status.ToString(),
            match.HomeGoals,
            match.AwayGoals,
            BettingWindow.IsBettingOpen(match.AllowBetUntil, utcClock.UtcNow)));
    }

    private static TeamSummaryResponse mapTeam(Team team)
    {
        return new TeamSummaryResponse(team.Id, team.Name, team.Code, team.FlagUrl);
    }

    private static DateTime ensureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
