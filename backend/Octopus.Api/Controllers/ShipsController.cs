using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/ships")]
public class ShipsController : ControllerBase
{
    private readonly ShipService _shipService;

    public ShipsController(ShipService shipService)
    {
        _shipService = shipService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_shipService.GetAll());
    }
}
