using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

/// <summary>
/// Retrieve dock (berth) information, assignments, and assign ships to docks.
/// </summary>
[ApiController]
[Route("api/docks")]
[Tags("Docks")]
public class BerthsController : ControllerBase
{
    private readonly DockService _dockService;
    private readonly AssignmentService _assignmentService;

    public BerthsController(DockService dockService, AssignmentService assignmentService)
    {
        _dockService = dockService;
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// List all docks with their current ship assignments.
    /// </summary>
    /// <returns>A list of docks and nested assignment details.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var docks = _dockService.GetAll().Select(d => new
        {
            d.Id,
            d.Name,
            d.Size,
            Assignments = d.Assignments.Select(a => new
            {
                a.Id,
                a.ShipId,
                a.DockId,
                a.StartDay,
                a.EndDay,
                Ship = a.Ship is null ? null : new
                {
                    a.Ship.Id,
                    a.Ship.Name,
                    a.Ship.Notes,
                    a.Ship.Size,
                    a.Ship.Status,
                    a.Ship.ArrivalDay,
                    a.Ship.Duration
                }
            })
        });

        return Ok(docks);
    }

    /// <summary>
    /// Assign a ship to a specific dock.
    /// </summary>
    /// <param name="dockId">The dock ID (from URL).</param>
    /// <param name="request">The ship ID to assign.</param>
    /// <returns>The created assignment.</returns>
    [HttpPost("{dockId:int}/assign")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Assign(int dockId, [FromBody] AssignShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var assignment = _assignmentService.AssignShip(request.ShipId, dockId);
        if (assignment is null)
        {
            return BadRequest(new { message = "Cannot assign ship: dock/ship not found, size mismatch, dock occupied, or ship not pending." });
        }

        return CreatedAtAction(nameof(GetAll), new { id = assignment.Id }, new
        {
            assignment.Id,
            assignment.ShipId,
            assignment.DockId,
            assignment.StartDay,
            assignment.EndDay
        });
    }

    /// <summary>
    /// List all assignments across all docks.
    /// </summary>
    /// <returns>A list of all assignments with ship details.</returns>
    [HttpGet("assignments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllAssignments()
    {
        var assignments = _assignmentService.GetAll().Select(a => new
        {
            a.Id,
            a.ShipId,
            a.DockId,
            a.StartDay,
            a.EndDay,
            Ship = a.Ship is null ? null : new
            {
                a.Ship.Id,
                a.Ship.Name,
                a.Ship.Notes,
                a.Ship.Size,
                a.Ship.Status,
                a.Ship.ArrivalDay,
                a.Ship.Duration
            }
        });

        return Ok(assignments);
    }
}
