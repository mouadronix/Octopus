using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AssignmentsController(AssignmentService assignments) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Assignment>> GetAll() => Ok(assignments.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Assignment> GetById(int id)
    {
        var assignment = assignments.GetById(id);
        return assignment is null ? NotFound() : Ok(assignment);
    }

    [HttpPost]
    public ActionResult<Assignment> Create([FromBody] CreateAssignmentRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var assignment = new Assignment
        {
            ShipId = request.ShipId,
            BerthId = request.BerthId,
            StartsAt = request.StartsAt,
            Status = request.Status
        };

        var created = assignments.Create(assignment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Assignment> Update(int id, [FromBody] UpdateAssignmentRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var updated = assignments.Update(id, assignment =>
        {
            if (request.ShipId.HasValue) assignment.ShipId = request.ShipId.Value;
            if (request.BerthId.HasValue) assignment.BerthId = request.BerthId.Value;
            if (request.StartsAt.HasValue) assignment.StartsAt = request.StartsAt.Value;
            if (request.EndsAt.HasValue) assignment.EndsAt = request.EndsAt.Value;
            if (request.Status is not null) assignment.Status = request.Status;
        });

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        return assignments.Delete(id) ? NoContent() : NotFound();
    }
}
