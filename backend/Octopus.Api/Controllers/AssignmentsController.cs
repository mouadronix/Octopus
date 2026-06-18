using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
     // Service
    private readonly AssignmentService _assignmentService;

    public AssignmentsController(AssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    // GET: api/assignments
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_assignmentService.GetAll().Select(ToDto));
    }

    [HttpPost]
    public IActionResult Create([FromBody] AssignShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var assignment = _assignmentService.AssignShip(request.ShipId, request.DockId);
        if (assignment == null)
        {
            return BadRequest(new { message = "Ship or dock not found, or the selected dock is occupied for this time range." });
        }

        return CreatedAtAction(nameof(GetAll), new { id = assignment.Id }, ToDto(assignment));
    }

    private static object ToDto(Assignment assignment)
    {
        return new
        {
            assignment.Id,
            assignment.ShipId,
            assignment.DockId,
            assignment.StartDay,
            assignment.EndDay,
            Ship = assignment.Ship is null
                ? null
                : new
                {
                    assignment.Ship.Id,
                    assignment.Ship.Name,
                    assignment.Ship.Notes,
                    assignment.Ship.Size,
                    assignment.Ship.Status,
                    assignment.Ship.ArrivalDay,
                    assignment.Ship.Duration
                }
        };
    }
}
