using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
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

    private static object ToDto(Models.Assignment assignment)
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
