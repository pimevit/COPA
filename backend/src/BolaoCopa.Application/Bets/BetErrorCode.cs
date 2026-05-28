namespace BolaoCopa.Application.Bets;

public enum BetErrorCode
{
    None = 0,
    MatchNotFound,
    BetNotFound,
    BettingWindowClosed,
    DuplicateBet,
    UserNotFound,
    PublicBetsNotAllowed
}
