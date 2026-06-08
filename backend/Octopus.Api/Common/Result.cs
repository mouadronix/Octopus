namespace Octopus.Api.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Result() { }

    public static Result<T> Ok(T value) => new Result<T>
    {
        IsSuccess = true,
        Value = value
    };

    public static Result<T> Fail(string code, string message) => new Result<T>
    {
        IsSuccess = false,
        ErrorCode = code,
        ErrorMessage = message
    };
}
