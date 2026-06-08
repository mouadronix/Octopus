using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/ships")]
public class ShipsController : ControllerBase
{
    private readonly ShipService _shipService;
    private readonly AssignmentService _assignmentService;

    public ShipsController(ShipService shipService, AssignmentService assignmentService)
    {
        _shipService = shipService;
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_shipService.GetAll());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var ship = _shipService.GetById(id);
        if (ship == null) return NotFound();
        return Ok(ship);
    }

    [HttpGet("{id:int}/suggestion")]
    public IActionResult GetSuggestion(int id)
    {
        var suggestion = _assignmentService.GetSuggestion(id);
        if (suggestion == null) return NotFound(new { message = "No dock available for this ship" });
        return Ok(suggestion);
    }
}
