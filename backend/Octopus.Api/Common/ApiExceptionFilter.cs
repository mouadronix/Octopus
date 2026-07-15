using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Octopus.Api.Common;

/// <summary>
/// Global exception filter that catches unhandled exceptions and returns a standardized ApiError response.
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception");

        var apiError = new ApiError(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred."
        );

        context.Result = new ObjectResult(apiError)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }
}
