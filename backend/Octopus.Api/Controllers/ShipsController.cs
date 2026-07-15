using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

/// <summary>
/// Manage ships and retrieve berth-assignment suggestions.
/// </summary>
[ApiController]
[Route("api/ships")]
[Tags("Ships")]
public class ShipsController : ControllerBase
{
    private readonly ShipService _shipService;
    private readonly AssignmentService _assignmentService;

    public ShipsController(ShipService shipService, AssignmentService assignmentService)
    {
        _shipService = shipService;
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// List all ships, optionally filtered by status.
    /// </summary>
    /// <param name="status">Optional ship status filter (e.g. Available, Pending).</param>
    /// <returns>A list of ships.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShipListItem>), StatusCodes.Status200OK)]
    public IActionResult GetAll([FromQuery] string? status)
    {
        var ships = _shipService.GetAll();
        if (status != null && Enum.TryParse<ShipStatus>(status, true, out var parsedStatus))
            ships = ships.Where(s => s.Status == parsedStatus).ToList();
        return Ok(ships.Select(ToListItem));
    }

    /// <summary>
    /// Get a single ship by its identifier.
    /// </summary>
    /// <param name="id">The ship ID.</param>
    /// <returns>The ship details.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ShipListItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        var ship = _shipService.GetById(id);
        if (ship == null) return NotFound();
        return Ok(ToListItem(ship));
    }

    /// <summary>
    /// Register a new ship in the terminal. Size, arrival day, and duration are auto-generated.
    /// </summary>
    /// <param name="request">Ship name and notes.</param>
    /// <returns>The created ship with auto-generated fields.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ShipListItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var ship = _shipService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = ship.Id }, ToListItem(ship));
    }

    /// <summary>
    /// Edit a ship's name and notes. Only allowed for ships with Pending status.
    /// </summary>
    /// <param name="id">The ship ID.</param>
    /// <param name="request">Updated name and notes.</param>
    /// <returns>The updated ship.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ShipListItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] UpdateShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var ship = _shipService.Update(id, request.Name, request.Notes);
        if (ship == null)
        {
            // Could be not found or not Pending — check which
            var existing = _shipService.GetById(id);
            if (existing == null) return NotFound();
            return BadRequest(new { message = "Only ships with Pending status can be edited." });
        }
        return Ok(ToListItem(ship));
    }

    /// <summary>
    /// Get a suggested dock assignment for a specific ship.
    /// </summary>
    /// <param name="id">The ship ID.</param>
    /// <returns>A suggestion containing the recommended dock, start day, and message.</returns>
    [HttpGet("{id:int}/suggest")]
    [ProducesResponseType(typeof(SuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetSuggestion(int id)
    {
        var suggestion = _assignmentService.GetSuggestion(id);
        if (suggestion == null) return NotFound(new { message = "No dock available for this ship" });
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
