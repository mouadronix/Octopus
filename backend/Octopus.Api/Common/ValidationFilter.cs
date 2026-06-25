using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Octopus.Api.Common;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(
                new ApiError(400, "Validation failed", errors));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
