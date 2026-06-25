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

        // Pre-fetch dock assignments, then call the scheduling module
        var dockAssignments = _context.Assignments
            .Where(a => a.DockId == dock.Id)
            .ToList();

        var (canAssign, startDay) = SchedulingModule.FindEarliestSlot(
            ship, dock, dockAssignments, terminal.CurrentDay, terminal.PlanningHorizon);

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

    /// <summary>
    /// Get a scheduling suggestion for a ship. Delegates all scheduling logic to SchedulingModule.
    /// </summary>
    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || terminal is null || ship.Status != ShipStatus.Pending)
            return null;

        var allDocks = _context.Docks.ToList();

        // Pre-fetch assignments grouped by dock
        var assignmentsByDock = _context.Assignments
            .GroupBy(a => a.DockId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return SchedulingModule.Suggest(
            ship, allDocks, assignmentsByDock,
            terminal.CurrentDay, terminal.PlanningHorizon);
    }
}
