using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.DTOs;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class ShipService
{
    private readonly AppDbContext _context;
    private static readonly Random _random = new();

    public ShipService(AppDbContext context)
    {
        _context = context;
    }

    public List<Ship> GetAll()
    {
        return _context.Ships
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Dock)
            .OrderBy(s => s.ArrivalDay)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public Ship? GetById(int id)
    {
        return _context.Ships
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Dock)
            .FirstOrDefault(s => s.Id == id);
    }

    /// <summary>
    /// Creates a new ship with auto-generated size, arrival day, and duration.
    /// Spec: Operator only enters name and notes.
    /// </summary>
    public Ship Create(CreateShipRequest request)
    {
        var terminal = _context.TerminalStates.FirstOrDefault();
        var currentDay = terminal?.CurrentDay ?? 1;

        var sizes = Enum.GetValues<ShipSize>();
        var size = sizes[_random.Next(sizes.Length)];
        var arrivalDay = currentDay + _random.Next(0, 31); // 0–30 days from current day
        var duration = _random.Next(3, 16); // 3–15 days

        var ship = new Ship
        {
            Name = request.Name,
            Notes = request.Notes,
            Size = size,
            ArrivalDay = arrivalDay,
            Duration = duration,
            Status = ShipStatus.Pending
        };

        _context.Ships.Add(ship);
        _context.SaveChanges();
        return ship;
    }

    /// <summary>
    /// Updates name/notes of a Pending ship.
    /// Spec: only Pending ships can be edited.
    /// </summary>
    public Ship? Update(int id, string name, string notes)
    {
        var ship = GetById(id);
        if (ship is null) return null;
        if (ship.Status != ShipStatus.Pending) return null;

        ship.Name = name;
        ship.Notes = notes;
        _context.SaveChanges();
        return ship;
    }
}
