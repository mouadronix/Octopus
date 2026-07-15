using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

/// <summary>
/// Query and advance the simulation's current day.
/// </summary>
[ApiController]
[Route("api/terminal")]
[Tags("Terminal")]
public class TerminalController : ControllerBase
{
    private readonly SystemService _systemService;

    public TerminalController(SystemService systemService)
    {
        _systemService = systemService;
    }

    /// <summary>
    /// Get the current simulation day.
    /// </summary>
    /// <returns>An object containing the current day number.</returns>
    [HttpGet("day")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCurrentDay()
    {
        return Ok(new { currentDay = _systemService.GetCurrentDay() });
    }

    /// <summary>
    /// Advance the simulation by one day.
    /// </summary>
    /// <returns>The updated state after advancing.</returns>
    [HttpPost("next-day")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult NextDay()
    {
        var state = _systemService.AdvanceDay();
        return Ok(new { currentDay = state.CurrentDay });
    }
}
