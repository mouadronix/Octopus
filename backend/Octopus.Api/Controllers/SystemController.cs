using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SystemController(SystemService system) : ControllerBase
{


    // GET: api/system/state
    [HttpGet("state")]
    public ActionResult<SystemState> GetState() => Ok(system.GetState());

    [HttpPost("advance-day")]
    public ActionResult<SystemState> AdvanceDay() => Ok(system.AdvanceDay());
}
