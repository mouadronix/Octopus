using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/docks")]
public class DocksController : ControllerBase
{
    private readonly DockService _dockService;
    private readonly AssignmentService _assignmentService;

    public DocksController(DockService dockService, AssignmentService assignmentService)
    {
        _dockService = dockService;
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_dockService.GetAll());
    }

    [HttpPost("{id:int}/assign")]
    public IActionResult Assign(int id, [FromBody] AssignShipRequest request)
    {
        var result = _assignmentService.AssignShip(id, request.ShipId);
        if (!result.IsSuccess)
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        return Ok(result.Value);
    }
}
