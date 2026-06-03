using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AssignmentsController(AssignmentService assignments) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Assignment>> GetAll() => Ok(assignments.GetAll());

    [HttpPost]
    public ActionResult<Assignment> Create(Assignment assignment)
    {
        var created = assignments.Create(assignment);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}
