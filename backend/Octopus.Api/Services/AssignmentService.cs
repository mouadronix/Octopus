using Microsoft.EntityFrameworkCore;
using Octopus.Api.Common;
using Octopus.Api.Data;
using Octopus.Api.DTOs;
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

    public (bool CanAssign, int StartDay) CanAssign(Ship ship, Dock dock, TerminalState terminal)
    {
        if (ship.ArrivalDay < terminal.CurrentDay)
            return (false, -1);

        var assignments = _context.Assignments
            .Where(a => a.DockId == dock.Id)
            .OrderBy(a => a.StartDay)
            .ToList();

        int candidate = Math.Max(ship.ArrivalDay, terminal.CurrentDay);
        int maxDay = terminal.CurrentDay + terminal.PlanningHorizon;

        foreach (var a in assignments)
        {
            int candidateEnd = candidate + ship.Duration - 1;
            if (candidateEnd < a.StartDay)
                return (true, candidate);
            candidate = a.EndDay + 1;
        }

        if (candidate + ship.Duration - 1 <= maxDay)
            return (true, candidate);

        return (false, -1);
    }

    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        if (ship == null || ship.Status != ShipStatus.Pending)
            return null;

        var terminal = _context.TerminalStates.First();

        var compatibleDocks = _context.Docks
            .Where(d => d.Size == ship.Size)
            .ToList();

        var best = compatibleDocks
            .Select(dock => new {
                Dock = dock,
                Result = CanAssign(ship, dock, terminal)
            })
            .Where(r => r.Result.CanAssign)
            .OrderBy(r => r.Result.StartDay)
            .ThenBy(r => _context.Assignments.Count(a => a.DockId == r.Dock.Id))
            .FirstOrDefault();

        if (best == null) return null;

        return new SuggestionResponse
        {
            DockId = best.Dock.Id,
            DockName = best.Dock.Name,
            StartDay = best.Result.StartDay,
            Message = best.Result.StartDay <= ship.ArrivalDay
                ? $"Available from Day {best.Result.StartDay}"
                : $"Delayed: earliest slot Day {best.Result.StartDay}"
        };
    }

    public Result<Assignment> AssignShip(int dockId, int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        if (ship == null)
            return Result<Assignment>.Fail("SHIP_NOT_FOUND", "Ship not found");
        if (ship.Status != ShipStatus.Pending)
            return Result<Assignment>.Fail("ALREADY_ASSIGNED", "Ship is not pending");

        var dock = _context.Docks.Find(dockId);
        if (dock == null)
            return Result<Assignment>.Fail("DOCK_NOT_FOUND", "Dock not found");
        if (dock.Size != ship.Size)
            return Result<Assignment>.Fail("SIZE_MISMATCH", "Dock size doesn't match ship size");

        var terminal = _context.TerminalStates.First();
        var (canAssign, startDay) = CanAssign(ship, dock, terminal);
        if (!canAssign)
            return Result<Assignment>.Fail("NO_SLOT", "No available slot for this ship");

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var assignment = new Assignment
            {
                ShipId = ship.Id,
                DockId = dock.Id,
                StartDay = startDay,
                EndDay = startDay + ship.Duration - 1
            };
            _context.Assignments.Add(assignment);
            ship.Status = ShipStatus.Assigned;
            _context.SaveChanges();
            transaction.Commit();
            return Result<Assignment>.Ok(assignment);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
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
