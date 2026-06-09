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

    public Ship? GetById(int id)
    {
        return _context.Ships.Find(id);
    }

    public Ship Create(string name, string notes)
    {
        var terminal = _context.TerminalStates.First();
        var random = new Random();
        var sizes = Enum.GetValues<ShipSize>();

        var ship = new Ship
        {
            Name = name,
            Notes = notes,
            Size = sizes[random.Next(sizes.Length)],
            ArrivalDay = terminal.CurrentDay + random.Next(0, 31),
            Duration = random.Next(3, 16),
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
