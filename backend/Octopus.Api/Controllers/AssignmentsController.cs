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

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_assignmentService.GetAll());
    }
}
