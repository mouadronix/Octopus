using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class ShipService(AppDbContext db)
{
    public IReadOnlyList<Ship> GetAll() => db.Ships;

    public Ship? GetById(int id) => db.Ships.FirstOrDefault(ship => ship.Id == id);

    public Ship Create(Ship ship)
    {
        ship.Id = db.Ships.Count == 0 ? 1 : db.Ships.Max(existing => existing.Id) + 1;
        db.Ships.Add(ship);
        return ship;
    }
}
