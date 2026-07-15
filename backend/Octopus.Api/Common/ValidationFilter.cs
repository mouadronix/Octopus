using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Octopus.Api.Common;

/// <summary>
/// Global filter that converts ModelState validation errors into a standardized ApiError response.
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var apiError = new ApiError(
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                errors
            );

            context.Result = new BadRequestObjectResult(apiError);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
