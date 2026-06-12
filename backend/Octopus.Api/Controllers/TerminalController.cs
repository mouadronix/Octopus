using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/terminal")]
public class TerminalController : ControllerBase
{
    private readonly SystemService _systemService;

    public TerminalController(SystemService systemService)
    {
        _systemService = systemService;
    }

    [HttpGet("day")]
    public IActionResult GetCurrentDay()
    {
        return Ok(new { currentDay = _systemService.GetCurrentDay() });
    }

    [HttpPost("next-day")]
    public IActionResult NextDay()
    {
        var newDay = _systemService.AdvanceDay();
        return Ok(new { currentDay = newDay });
    }
}
