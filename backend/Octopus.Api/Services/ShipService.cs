using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class ShipService(AppDbContext db)
{
    public IReadOnlyList<Ship> GetAll() =>
        db.Ships
            .OrderBy(ship => ship.ArrivalDay)
            .ThenBy(ship => ship.Name)
            .AsNoTracking()
            .ToList();

    public Ship? GetById(int id) =>
        db.Ships
            .Include(ship => ship.Assignments)
            .AsNoTracking()
            .FirstOrDefault(ship => ship.Id == id);

    public Ship Create(Ship ship)
    {
        ship.Status = ShipStatus.Pending;
        db.Ships.Add(ship);
        db.SaveChanges();
        return ship;
    }
}
