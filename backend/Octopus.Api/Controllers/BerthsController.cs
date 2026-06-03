using Microsoft.AspNetCore.Mvc;
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
}
