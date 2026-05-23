namespace BolaoCopa.Application.Bets;

public sealed class BetResult<T>
{
    private BetResult(T? value, BetErrorCode errorCode, string? errorMessage)
    {
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public T? Value { get; }
    public BetErrorCode ErrorCode { get; }
    public string? ErrorMessage { get; }
    public bool Succeeded => ErrorCode == BetErrorCode.None;

    public static BetResult<T> Success(T value)
    {
        return new BetResult<T>(value, BetErrorCode.None, null);
    }

    public static BetResult<T> Failure(BetErrorCode errorCode, string errorMessage)
    {
        return new BetResult<T>(default, errorCode, errorMessage);
    }
}
