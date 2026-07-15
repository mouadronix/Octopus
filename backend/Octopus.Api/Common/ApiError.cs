namespace Octopus.Api.Common;

public class ApiError
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ApiError(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public ApiError(int statusCode, string message, Dictionary<string, string[]> errors)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }
}
