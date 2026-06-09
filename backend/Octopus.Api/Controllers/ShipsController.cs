using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
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
    public IActionResult GetAll([FromQuery] string? status)
    {
        var ships = _shipService.GetAll();
        if (status != null && Enum.TryParse<ShipStatus>(status, true, out var parsedStatus))
            ships = ships.Where(s => s.Status == parsedStatus).ToList();
        return Ok(ships);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var ship = _shipService.GetById(id);
        if (ship == null) return NotFound();
        return Ok(ship);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var ship = _shipService.Create(request.Name, request.Notes);
        return CreatedAtAction(nameof(GetById), new { id = ship.Id }, ship);
    }

    [HttpGet("{id:int}/suggestion")]
    public IActionResult GetSuggestion(int id)
    {
        var suggestion = _assignmentService.GetSuggestion(id);
        if (suggestion == null) return NotFound(new { message = "No dock available for this ship" });
        return Ok(suggestion);
    }
}
