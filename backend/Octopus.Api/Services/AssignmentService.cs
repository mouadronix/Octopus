using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.DTOs;
using Octopus.Api.Models;

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
            .OrderBy(a => a.StartDay)
            .ToList();
    }

    public (bool CanAssign, int StartDay) CanAssign(Ship ship, Dock dock, TerminalState terminal)
    {
        if (ship.ArrivalDay < terminal.CurrentDay)
            return (false, -1);

        if (ship.Duration < 1)
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

        int finalEnd = candidate + ship.Duration - 1;
        return finalEnd <= maxDay ? (true, candidate) : (false, -1);
    }

    public Assignment? AssignShip(int shipId, int dockId)
    {
        var ship = _context.Ships
            .Include(s => s.Assignment)
            .FirstOrDefault(s => s.Id == shipId);
        var dock = _context.Docks.Find(dockId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || dock is null || terminal is null)
            return null;

        if (ship.Status != ShipStatus.Pending || ship.Assignment is not null)
            return null;

        if (dock.Size != ship.Size)
            return null;

        var (canAssign, startDay) = CanAssign(ship, dock, terminal);
        if (!canAssign)
            return null;

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

            ship.Status = ShipStatus.Assigned;
            _context.Assignments.Add(assignment);
            _context.SaveChanges();
            transaction.Commit();

            return assignment;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || terminal is null || ship.Status != ShipStatus.Pending)
            return null;

        var compatibleDocks = _context.Docks
            .Where(d => d.Size == ship.Size)
            .ToList();

        // Find earliest assignable day for each compatible dock
        var candidates = new List<(Dock Dock, int StartDay, int AssignmentCount)>();
        foreach (var dock in compatibleDocks)
        {
            var (canAssign, startDay) = CanAssign(ship, dock, terminal);
            if (canAssign)
            {
                int count = _context.Assignments.Count(a => a.DockId == dock.Id);
                candidates.Add((dock, startDay, count));
            }
        }

        if (candidates.Count == 0)
            return null;

        // Pick earliest start day, tie-break by fewest existing assignments
        var best = candidates
            .OrderBy(c => c.StartDay)
            .ThenBy(c => c.AssignmentCount)
            .First();

        return new SuggestionResponse
        {
            DockId = best.Dock.Id,
            DockName = best.Dock.Name,
            StartDay = best.StartDay,
            Message = best.StartDay <= ship.ArrivalDay
                ? $"Available from Day {best.StartDay}"
                : $"Delayed: earliest slot Day {best.StartDay}"
        };
    }
}
