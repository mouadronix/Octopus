using Microsoft.AspNetCore.Mvc;
using Octopus.Api.Services;

namespace Octopus.Api.Controllers;

[ApiController]
[Route("api/docks")]
public class BerthsController : ControllerBase
{


    //Service
    private readonly DockService _dockService;

    public BerthsController(DockService dockService)
    {
        _dockService = dockService;
    }


    // GET: api/docks
    [HttpGet]
    public IActionResult GetAll()
    {
        var docks = _dockService.GetAll().Select(d => new
        {
            d.Id,
            d.Name,
            d.Size,
            Assignments = d.Assignments.Select(a => new
            {
                a.Id,
                a.ShipId,
                a.DockId,
                a.StartDay,
                a.EndDay,
                Ship = new
                {
                    a.Ship.Id,
                    a.Ship.Name,
                    a.Ship.Notes,
                    a.Ship.Size,
                    a.Ship.Status,
                    a.Ship.ArrivalDay,
                    a.Ship.Duration
                }
            })
        });

        return Ok(docks);
    }
}
