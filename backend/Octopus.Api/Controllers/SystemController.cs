using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SystemController(ISystemService system) : ControllerBase
{
    [HttpGet("state")]
    public ActionResult<SystemState> GetState() => Ok(system.GetState());
}
