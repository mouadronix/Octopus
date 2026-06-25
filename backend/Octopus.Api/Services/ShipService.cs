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
    /// Creates a new ship with auto-generated Size, ArrivalDay, and Duration.
    /// Operator only provides Name and Notes.
    /// </summary>
    public Ship Create(CreateShipRequest request)
    {
        var terminal = _context.TerminalStates.FirstOrDefault();
        var currentDay = terminal?.CurrentDay ?? 1;

        var sizes = Enum.GetValues<ShipSize>();

        var ship = new Ship
        {
            Name = request.Name,
            Notes = request.Notes,
            Size = sizes[_random.Next(sizes.Length)],
            ArrivalDay = currentDay + _random.Next(0, 31),
            Duration = _random.Next(3, 16),
            Status = ShipStatus.Pending
        };

        _context.Ships.Add(ship);
        _context.SaveChanges();
        return ship;
    }

    /// <summary>
    /// Updates name and notes of a Pending ship. Returns null if not found or not Pending.
    /// </summary>
    public Ship? UpdateNameNotes(int id, EditShipRequest request)
    {
        var ship = _context.Ships.Find(id);
        if (ship is null || ship.Status != ShipStatus.Pending)
            return null;

        ship.Name = request.Name;
        ship.Notes = request.Notes;
        _context.SaveChanges();
        return ship;
    }

    public bool Delete(int id)
    {
        var ship = _context.Ships.Find(id);
        if (ship is null) return false;
        _context.Ships.Remove(ship);
        _context.SaveChanges();
        return true;
    }
}
