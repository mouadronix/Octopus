using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AssignmentsController(IAssignmentService assignments) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Assignment>>> GetAll(CancellationToken ct)
        => Ok(await assignments.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Assignment>> GetById(int id, CancellationToken ct)
    {
        var assignment = await assignments.GetByIdAsync(id, ct);
        return assignment is null ? NotFound() : Ok(assignment);
    }

    [HttpPost]
    public async Task<ActionResult<Assignment>> Create([FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var assignment = new Assignment
        {
            ShipId = request.ShipId,
            BerthId = request.BerthId,
            StartsAt = request.StartsAt,
            Status = request.Status
        };

        var created = await assignments.CreateAsync(assignment, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Assignment>> Update(int id, [FromBody] UpdateAssignmentRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var updated = await assignments.UpdateAsync(id, assignment =>
        {
            if (request.ShipId.HasValue) assignment.ShipId = request.ShipId.Value;
            if (request.BerthId.HasValue) assignment.BerthId = request.BerthId.Value;
            if (request.StartsAt.HasValue) assignment.StartsAt = request.StartsAt.Value;
            if (request.EndsAt.HasValue) assignment.EndsAt = request.EndsAt.Value;
            if (request.Status is not null) assignment.Status = request.Status;
        }, ct);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        return await assignments.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
