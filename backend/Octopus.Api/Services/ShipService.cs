using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class ShipService
{
    private readonly AppDbContext _context;

    public ShipService(AppDbContext context)
    {
        _context = context;
    }

    public List<Ship> GetAll()
    {
        return _context.Ships.ToList();
    }

    public Ship? Update(int id, Action<Ship> apply)
    {
        var ship = GetById(id);
        if (ship is null) return null;
        apply(ship);
        return ship;
    }

    public bool Delete(int id)
    {
        var ship = GetById(id);
        if (ship is null) return false;
        db.Ships.Remove(ship);
        return true;
    }
}
