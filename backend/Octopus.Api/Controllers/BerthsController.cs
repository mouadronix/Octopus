using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/docks")]
public class BerthsController : ControllerBase
{
    private readonly DockService _dockService;

    public BerthsController(DockService dockService)
    {
        _dockService = dockService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_dockService.GetAll());
    }
}
