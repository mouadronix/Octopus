using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Common;
using Octopus.Api.DTOs;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/docks")]
public class BerthsController : ControllerBase
{
    private readonly DockService _dockService;
    private readonly AssignmentService _assignmentService;

    public BerthsController(DockService dockService, AssignmentService assignmentService)
    {
        _dockService = dockService;
        _assignmentService = assignmentService;
    }

    // GET: api/docks
    [HttpGet]
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

    // POST: api/docks/1/assign
    [HttpPost("{id:int}/assign")]
    public IActionResult AssignShip(int id, [FromBody] AssignShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var assignment = _assignmentService.AssignShip(request.ShipId, id);
        if (assignment is null)
        {
            return BadRequest(new ApiError(400,
                "Cannot assign ship: dock/ship not found, size mismatch, dock occupied, or ship not pending."));
        }

        return Ok(new
        {
            assignment.Id,
            assignment.ShipId,
            assignment.DockId,
            assignment.StartDay,
            assignment.EndDay
        });
    }
}
