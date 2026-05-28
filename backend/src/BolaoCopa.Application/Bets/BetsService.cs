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

        if (!BettingWindow.IsBettingOpen(match.AllowBetUntil, utcClock.UtcNow, match.IsBettingLocked))
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

        if (!BettingWindow.IsBettingOpen(bet.Match.AllowBetUntil, utcClock.UtcNow, bet.Match.IsBettingLocked))
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

    public async Task<BetResult<BetVisibilityResponse>> GetVisibilityAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await betRepository.FindUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return BetResult<BetVisibilityResponse>.Failure(
                BetErrorCode.UserNotFound,
                "User was not found.");
        }

        return BetResult<BetVisibilityResponse>.Success(new BetVisibilityResponse(user.ShowBetsPublicly));
    }

    public async Task<BetResult<BetVisibilityResponse>> UpdateVisibilityAsync(
        int userId,
        UpdateBetVisibilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await betRepository.FindUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return BetResult<BetVisibilityResponse>.Failure(
                BetErrorCode.UserNotFound,
                "User was not found.");
        }

        user.ShowBetsPublicly = request.ShowBetsPublicly;

        await betRepository.SaveChangesAsync(cancellationToken);

        return BetResult<BetVisibilityResponse>.Success(new BetVisibilityResponse(user.ShowBetsPublicly));
    }

    public async Task<BetResult<IReadOnlyList<PublicBetResponse>>> ListPublicAsync(
        int userId,
        int? matchId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await betRepository.FindUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return BetResult<IReadOnlyList<PublicBetResponse>>.Failure(
                BetErrorCode.UserNotFound,
                "User was not found.");
        }

        if (!user.ShowBetsPublicly)
        {
            return BetResult<IReadOnlyList<PublicBetResponse>>.Failure(
                BetErrorCode.PublicBetsNotAllowed,
                "Make your bets public to see public bets from other players.");
        }

        if (matchId.HasValue && !await betRepository.MatchExistsAsync(matchId.Value, cancellationToken))
        {
            return BetResult<IReadOnlyList<PublicBetResponse>>.Failure(
                BetErrorCode.MatchNotFound,
                "Match was not found.");
        }

        var bets = await betRepository.ListPublicAsync(matchId, cancellationToken);
        var response = bets
            .Select(bet => mapPublicBet(bet, userId))
            .ToList();

        return BetResult<IReadOnlyList<PublicBetResponse>>.Success(response);
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

    private static PublicBetResponse mapPublicBet(Bet bet, int currentUserId)
    {
        if (bet.User is null)
        {
            throw new InvalidOperationException("Bet user must be loaded before mapping.");
        }

        return new PublicBetResponse(
            bet.MatchId,
            bet.UserId,
            bet.User.Name,
            bet.HomeGoalsPrediction,
            bet.AwayGoalsPrediction,
            bet.PointsEarned,
            bet.CreatedAt,
            bet.UserId == currentUserId);
    }
}
