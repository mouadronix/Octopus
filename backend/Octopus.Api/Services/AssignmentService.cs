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

    /// <summary>
    /// Assign a ship to a specific dock.
    /// Spec: uses database transaction, checks planning horizon.
    /// </summary>
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

        if (!CanFitShip(dock.Size, ship.Size))
            return null;

        var earliestDay = Math.Max(ship.ArrivalDay, terminal.CurrentDay);
        var horizonEnd = terminal.CurrentDay + terminal.PlanningHorizon;

        // Find earliest available window (may be later than arrival if dock is occupied)
        var slot = FindEarliestSlot(dock.Id, earliestDay, ship.Duration, horizonEnd);
        if (slot is null)
            return null;

        var startDay = slot.Value.start;
        var endDay = slot.Value.end;

        // Spec: wrap assignment in a database transaction
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var assignment = new Assignment
            {
                ShipId = ship.Id,
                DockId = dock.Id,
                StartDay = startDay,
                EndDay = endDay
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

    /// <summary>
    /// Get a suggested dock for a ship using first-fit greedy algorithm.
    /// Spec: scan docks in order, take the first that fits.
    /// </summary>
    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || terminal is null || ship.Status != ShipStatus.Pending)
            return null;

        var earliestDay = Math.Max(ship.ArrivalDay, terminal.CurrentDay);
        var horizonEnd = terminal.CurrentDay + terminal.PlanningHorizon;

        if (earliestDay + ship.Duration - 1 > horizonEnd)
            return null;

        // First-fit: scan docks in natural DB order (by Id), take first with an available slot
        var dock = _context.Docks
            .AsEnumerable()
            .Where(d => CanFitShip(d.Size, ship.Size))
            .Select(d => new { Dock = d, Slot = FindEarliestSlot(d.Id, earliestDay, ship.Duration, horizonEnd) })
            .Where(x => x.Slot is not null)
            .FirstOrDefault();

        if (dock is null)
            return null;

        var startDay = dock.Slot!.Value.start;

        return new SuggestionResponse
        {
            DockId = dock.Dock.Id,
            DockName = dock.Dock.Name,
            StartDay = startDay,
            Message = startDay <= ship.ArrivalDay
                ? $"Available from Day {startDay}"
                : $"Delayed: earliest slot Day {startDay}"
        };
    }

    private bool HasDockConflict(int dockId, int startDay, int endDay)
    {
        return _context.Assignments.Any(a =>
            a.DockId == dockId &&
            a.StartDay <= endDay &&
            a.EndDay >= startDay);
    }

    /// <summary>
    /// Find the earliest window [start, start+duration-1] on a dock with no conflicts.
    /// Walks through existing assignments looking for gaps large enough to fit the ship.
    /// </summary>
    private (int start, int end)? FindEarliestSlot(int dockId, int earliestDay, int duration, int horizonEnd)
    {
        var assignments = _context.Assignments
            .Where(a => a.DockId == dockId)
            .OrderBy(a => a.StartDay)
            .Select(a => new { a.StartDay, a.EndDay })
            .ToList();

        var candidate = earliestDay;

        foreach (var a in assignments)
        {
            // Try to fit before this assignment
            if (candidate + duration - 1 < a.StartDay)
                return (candidate, candidate + duration - 1);

            // Jump past this assignment
            candidate = Math.Max(candidate, a.EndDay + 1);
        }

        // Try after all assignments
        if (candidate + duration - 1 <= horizonEnd)
            return (candidate, candidate + duration - 1);

        return null;
    }

    private static bool CanFitShip(ShipSize dockSize, ShipSize shipSize)
    {
        return SizeRank(dockSize) >= SizeRank(shipSize);
    }

    private static int SizeRank(ShipSize size)
    {
        return size switch
        {
            ShipSize.S => 1,
            ShipSize.M => 2,
            ShipSize.L => 3,
            ShipSize.XL => 4,
            _ => 0
        };
    }
}
