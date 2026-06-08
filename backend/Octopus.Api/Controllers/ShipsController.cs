using Microsoft.AspNetCore.Mvc;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShipsController(ShipService ships) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Ship>> GetAll() => Ok(ships.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Ship> GetById(int id)
    {
        var ship = ships.GetById(id);
        return ship is null ? NotFound() : Ok(ship);
    }

    [HttpPost]
    public ActionResult<Ship> Create([FromBody] CreateShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var ship = new Ship
        {
            Name = request.Name,
            ImoNumber = request.ImoNumber,
            CargoType = request.CargoType,
            EstimatedArrival = request.EstimatedArrival
        };

        var created = ships.Create(ship);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Ship> Update(int id, [FromBody] UpdateShipRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var updated = ships.Update(id, ship =>
        {
            if (request.Name is not null) ship.Name = request.Name;
            if (request.ImoNumber is not null) ship.ImoNumber = request.ImoNumber;
            if (request.CargoType is not null) ship.CargoType = request.CargoType;
            if (request.EstimatedArrival.HasValue) ship.EstimatedArrival = request.EstimatedArrival.Value;
            if (request.Status is not null) ship.Status = request.Status;
        });

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        return ships.Delete(id) ? NoContent() : NotFound();
    }
}
