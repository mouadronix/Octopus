namespace Octopus.Api.Common;

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
