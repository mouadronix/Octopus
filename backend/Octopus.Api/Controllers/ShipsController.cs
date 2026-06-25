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

    // GET: api/ships?status=Pending
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? status)
    {
        var ships = _shipService.GetAll();
        if (status != null && Enum.TryParse<ShipStatus>(status, true, out var parsedStatus))
            ships = ships.Where(s => s.Status == parsedStatus).ToList();
        return Ok(ships.Select(ToListItem));
    }

    // GET: api/ships/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var ship = _shipService.GetById(id);
        if (ship is null) return NotFound();
        return Ok(ToListItem(ship));
    }

    // POST: api/ships
    [HttpPost]
    public IActionResult Create([FromBody] CreateShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var ship = _shipService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = ship.Id }, ToListItem(ship));
    }

    // PUT: api/ships/1 — edit name/notes (Pending only)
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] EditShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var ship = _shipService.UpdateNameNotes(id, request);
        if (ship is null)
            return NotFound(new { message = "Ship not found or not in Pending status" });
        return Ok(ToListItem(ship));
    }

    // GET: api/ships/1/suggest
    [HttpGet("{id:int}/suggest")]
    public IActionResult GetSuggestion(int id)
    {
        var suggestion = _assignmentService.GetSuggestion(id);
        if (suggestion is null)
            return NotFound(new { message = "No dock available for this ship" });
        return Ok(suggestion);
    }

    private static ShipListItem ToListItem(Ship ship)
    {
        return new ShipListItem
        {
            Id = ship.Id,
            Name = ship.Name,
            Notes = ship.Notes,
            Size = ship.Size,
            Status = ship.Status,
            ArrivalDay = ship.ArrivalDay,
            Duration = ship.Duration,
            BerthName = ship.Assignment?.Dock?.Name,
            AssignmentId = ship.Assignment?.Id,
            AssignmentStartDay = ship.Assignment?.StartDay,
            AssignmentEndDay = ship.Assignment?.EndDay
        };
    }
}
