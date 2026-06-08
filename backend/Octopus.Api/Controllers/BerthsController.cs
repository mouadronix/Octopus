using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BerthsController(BerthService berths) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Berth>> GetAll() => Ok(berths.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Berth> GetById(int id)
    {
        var berth = berths.GetById(id);
        return berth is null ? NotFound() : Ok(berth);
    }

    [HttpPost]
    public ActionResult<Berth> Create([FromBody] CreateBerthRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var berth = new Berth
        {
            Name = request.Name,
            MaxDraftMeters = request.MaxDraftMeters
        };

        var created = berths.Create(berth);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Berth> Update(int id, [FromBody] UpdateBerthRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var updated = berths.Update(id, berth =>
        {
            if (request.Name is not null) berth.Name = request.Name;
            if (request.MaxDraftMeters.HasValue) berth.MaxDraftMeters = request.MaxDraftMeters.Value;
            if (request.IsAvailable.HasValue) berth.IsAvailable = request.IsAvailable.Value;
        });

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        return berths.Delete(id) ? NoContent() : NotFound();
    }
}
