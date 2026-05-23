using BolaoCopa.Application.Bets.Contracts;
using BolaoCopa.Application.Bets.Data;
using BolaoCopa.Application.Common.Time;
using BolaoCopa.Application.Matches;
using BolaoCopa.Application.Matches.Contracts;
using BolaoCopa.Domain.Entities;

namespace BolaoCopa.Application.Bets;

public sealed class BetsService(
    IBetRepository betRepository,
    IUtcClock utcClock)
{
    public async Task<BetResult<BetResponse>> CreateAsync(
        int userId,
        CreateBetRequest request,
        CancellationToken cancellationToken = default)
    {
        var match = await betRepository.FindMatchByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
        {
            return BetResult<BetResponse>.Failure(
                BetErrorCode.MatchNotFound,
                "Match was not found.");
        }

        if (!BettingWindow.IsBettingOpen(match.AllowBetUntil, utcClock.UtcNow))
        {
            return BetResult<BetResponse>.Failure(
                BetErrorCode.BettingWindowClosed,
                "Betting window is closed for this match.");
        }

        var existingBet = await betRepository.FindByUserAndMatchAsync(userId, request.MatchId, cancellationToken);
        if (existingBet is not null)
        {
            return BetResult<BetResponse>.Failure(
                BetErrorCode.DuplicateBet,
                "User already has a bet for this match.");
        }

        var bet = new Bet
        {
            UserId = userId,
            MatchId = request.MatchId,
            HomeGoalsPrediction = request.HomeGoalsPrediction,
            AwayGoalsPrediction = request.AwayGoalsPrediction,
            PointsEarned = 0,
            CreatedAt = utcClock.UtcNow,
            Match = match
        };

        await betRepository.AddAsync(bet, cancellationToken);
        await betRepository.SaveChangesAsync(cancellationToken);

        return BetResult<BetResponse>.Success(mapBet(bet));
    }

    public async Task<BetResult<BetResponse>> UpdateAsync(
        int userId,
        int betId,
        UpdateBetRequest request,
        CancellationToken cancellationToken = default)
    {
        var bet = await betRepository.FindByIdAndUserAsync(betId, userId, cancellationToken);
        if (bet?.Match is null)
        {
            return BetResult<BetResponse>.Failure(
                BetErrorCode.BetNotFound,
                "Bet was not found for the authenticated user.");
        }

        if (!BettingWindow.IsBettingOpen(bet.Match.AllowBetUntil, utcClock.UtcNow))
        {
            return BetResult<BetResponse>.Failure(
                BetErrorCode.BettingWindowClosed,
                "Betting window is closed for this match.");
        }

        bet.HomeGoalsPrediction = request.HomeGoalsPrediction;
        bet.AwayGoalsPrediction = request.AwayGoalsPrediction;

        await betRepository.SaveChangesAsync(cancellationToken);

        return BetResult<BetResponse>.Success(mapBet(bet));
    }

    public async Task<IReadOnlyList<BetResponse>> ListMineAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var bets = await betRepository.ListByUserAsync(userId, cancellationToken);

        return bets
            .Select(mapBet)
            .ToList();
    }

    private static BetResponse mapBet(Bet bet)
    {
        if (bet.Match?.HomeTeam is null || bet.Match.AwayTeam is null)
        {
            throw new InvalidOperationException("Bet match and teams must be loaded before mapping.");
        }

        return new BetResponse(
            bet.Id,
            bet.MatchId,
            bet.HomeGoalsPrediction,
            bet.AwayGoalsPrediction,
            bet.PointsEarned,
            bet.CreatedAt,
            new BetMatchResponse(
                bet.Match.Id,
                new TeamSummaryResponse(
                    bet.Match.HomeTeam.Id,
                    bet.Match.HomeTeam.Name,
                    bet.Match.HomeTeam.Code,
                    bet.Match.HomeTeam.FlagUrl),
                new TeamSummaryResponse(
                    bet.Match.AwayTeam.Id,
                    bet.Match.AwayTeam.Name,
                    bet.Match.AwayTeam.Code,
                    bet.Match.AwayTeam.FlagUrl),
                bet.Match.MatchDate,
                bet.Match.Stage.ToString(),
                bet.Match.Status.ToString(),
                bet.Match.HomeGoals,
                bet.Match.AwayGoals));
    }
}
