using Microsoft.AspNetCore.Mvc;
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
    public ActionResult<Ship> Create(Ship ship)
    {
        var created = ships.Create(ship);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
