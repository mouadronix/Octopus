using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
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
        return Ok(_assignmentService.GetAll());
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

        return CreatedAtAction(nameof(GetAll), new { id = assignment.Id }, assignment);
    }
}
