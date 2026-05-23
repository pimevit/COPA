namespace BolaoCopa.Application.Matches;

public sealed class MatchResult<T>
{
    private MatchResult(T? value, MatchResultErrorCode errorCode, string? errorMessage)
    {
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public T? Value { get; }
    public MatchResultErrorCode ErrorCode { get; }
    public string? ErrorMessage { get; }
    public bool Succeeded => ErrorCode == MatchResultErrorCode.None;

    public static MatchResult<T> Success(T value)
    {
        return new MatchResult<T>(value, MatchResultErrorCode.None, null);
    }

    public static MatchResult<T> Failure(MatchResultErrorCode errorCode, string errorMessage)
    {
        return new MatchResult<T>(default, errorCode, errorMessage);
    }
}
