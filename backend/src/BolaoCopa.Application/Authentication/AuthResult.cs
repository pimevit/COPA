namespace BolaoCopa.Application.Authentication;

public sealed class AuthResult<T>
{
    private AuthResult(T? value, AuthErrorCode errorCode, string? errorMessage)
    {
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public T? Value { get; }
    public AuthErrorCode ErrorCode { get; }
    public string? ErrorMessage { get; }
    public bool Succeeded => ErrorCode == AuthErrorCode.None;

    public static AuthResult<T> Success(T value)
    {
        return new AuthResult<T>(value, AuthErrorCode.None, null);
    }

    public static AuthResult<T> Failure(AuthErrorCode errorCode, string errorMessage)
    {
        return new AuthResult<T>(default, errorCode, errorMessage);
    }
}
