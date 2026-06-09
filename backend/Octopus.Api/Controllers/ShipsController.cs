using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShipsController(IShipService ships) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Ship>>> GetAll(CancellationToken ct)
        => Ok(await ships.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ship>> GetById(int id, CancellationToken ct)
    {
        var ship = await ships.GetByIdAsync(id, ct);
        return ship is null ? NotFound() : Ok(ship);
    }

    [HttpPost]
    public async Task<ActionResult<Ship>> Create([FromBody] CreateShipRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var ship = new Ship
        {
            Name = request.Name,
            ImoNumber = request.ImoNumber,
            CargoType = request.CargoType,
            EstimatedArrival = request.EstimatedArrival
        };

        var created = await ships.CreateAsync(ship, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Ship>> Update(int id, [FromBody] UpdateShipRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var updated = await ships.UpdateAsync(id, ship =>
        {
            if (request.Name is not null) ship.Name = request.Name;
            if (request.ImoNumber is not null) ship.ImoNumber = request.ImoNumber;
            if (request.CargoType is not null) ship.CargoType = request.CargoType;
            if (request.EstimatedArrival.HasValue) ship.EstimatedArrival = request.EstimatedArrival.Value;
            if (request.Status is not null) ship.Status = request.Status;
        }, ct);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        return await ships.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
