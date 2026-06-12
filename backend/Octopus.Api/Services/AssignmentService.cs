using Octopus.Api.Data;
using Octopus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Octopus.Api.Services;

public class AssignmentService
{
    private readonly AppDbContext _context;

    public AssignmentService(AppDbContext context)
    {
        _context = context;
    }

    public List<Assignment> GetAll()
    {
        return _context.Assignments
            .Include(a => a.Ship)
            .Include(a => a.Dock)
            .ToList();
    }

    public Assignment? AssignShip(int shipId, int dockId)
    {
        var ship = _context.Ships.Find(shipId);
        var dock = _context.Docks.Find(dockId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship == null || dock == null) return null;

        var startDay = ship.ArrivalDay;
        var endDay = ship.ArrivalDay + ship.Duration;
        var hasConflict = _context.Assignments.Any(a =>
            a.DockId == dockId &&
            a.StartDay < endDay &&
            a.EndDay > startDay);

        if (hasConflict) return null;

        var existingAssignment = _context.Assignments.FirstOrDefault(a => a.ShipId == shipId);
        if (existingAssignment != null)
        {
            existingAssignment.DockId = dockId;
            existingAssignment.StartDay = startDay;
            existingAssignment.EndDay = endDay;
            ship.Status = ShipStatus.Assigned;
            _context.SaveChanges();
            return existingAssignment;
        }

        var assignment = new Assignment
        {
            ShipId = shipId,
            DockId = dockId,
            StartDay = startDay,
            EndDay = endDay
        };

        ship.Status = ShipStatus.Assigned;
        _context.Assignments.Add(assignment);

        if (terminal != null && terminal.CurrentDay < startDay)
        {
            terminal.CurrentDay = Math.Min(terminal.CurrentDay, startDay);
        }

        _context.SaveChanges();
        return assignment;
    }


    // Suggests an available dock for a ship
    public Dock? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        if (ship == null) return null;

        // Find a dock that matches the ship's size and has no conflicting assignments
        return _context.Docks
            .Where(d => d.Size == ship.Size).AsEnumerable().FirstOrDefault(d => !_context.Assignments.Any
            (a => a.DockId == d.Id &&
                    a.StartDay < ship.ArrivalDay + ship.Duration &&
                    a.EndDay > ship.ArrivalDay
            ));
    }
}
