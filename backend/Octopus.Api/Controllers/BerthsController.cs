using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BerthsController(IBerthService berths) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Berth>>> GetAll(CancellationToken ct)
        => Ok(await berths.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Berth>> GetById(int id, CancellationToken ct)
    {
        var berth = await berths.GetByIdAsync(id, ct);
        return berth is null ? NotFound() : Ok(berth);
    }

    [HttpPost]
    public async Task<ActionResult<Berth>> Create([FromBody] CreateBerthRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var berth = new Berth
        {
            Name = request.Name,
            MaxDraftMeters = request.MaxDraftMeters
        };

        var created = await berths.CreateAsync(berth, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Berth>> Update(int id, [FromBody] UpdateBerthRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var updated = await berths.UpdateAsync(id, berth =>
        {
            if (request.Name is not null) berth.Name = request.Name;
            if (request.MaxDraftMeters.HasValue) berth.MaxDraftMeters = request.MaxDraftMeters.Value;
            if (request.IsAvailable.HasValue) berth.IsAvailable = request.IsAvailable.Value;
        }, ct);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        return await berths.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
