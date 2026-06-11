using Octopus.Api.Data;
using Octopus.Api.DTOs;
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

    public Ship? GetById(int id)
    {
        return _context.Ships.Find(id);
    }

    public Ship Create(CreateShipRequest request)
    {
        var ship = new Ship
        {
            Name = request.Name,
            Notes = request.Notes,
            Size = request.Size,
            ArrivalDay = request.ArrivalDay,
            Duration = request.Duration,
            Status = ShipStatus.Pending
        };

        _context.Ships.Add(ship);
        _context.SaveChanges();
        return ship;
    }

    public Ship? Update(int id, Action<Ship> apply)
    {
        var ship = GetById(id);
        if (ship == null) return null;
        apply(ship);
        _context.SaveChanges();
        return ship;
    }

    public bool Delete(int id)
    {
        var ship = GetById(id);
        if (ship == null) return false;
        _context.Ships.Remove(ship);
        _context.SaveChanges();
        return true;
    }
}
