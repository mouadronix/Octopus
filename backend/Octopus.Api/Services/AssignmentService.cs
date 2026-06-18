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
        {
            return null;
        }

        if (ship.Status != ShipStatus.Pending || ship.Assignment is not null)
        {
            return null;
        }

        if (!CanFitShip(dock.Size, ship.Size))
        {
            return null;
        }

        var startDay = Math.Max(ship.ArrivalDay, terminal.CurrentDay);
        var endDay = startDay + ship.Duration - 1;

        if (HasDockConflict(dock.Id, startDay, endDay))
        {
            return null;
        }

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

        return assignment;
    }

    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || terminal is null || ship.Status != ShipStatus.Pending)
        {
            return null;
        }

        var startDay = Math.Max(ship.ArrivalDay, terminal.CurrentDay);
        var endDay = startDay + ship.Duration - 1;

        var dock = _context.Docks
            .AsEnumerable()
            .Where(d => CanFitShip(d.Size, ship.Size))
            .Where(d => !HasDockConflict(d.Id, startDay, endDay))
            .OrderBy(d => SizeRank(d.Size))
            .ThenBy(d => d.Name)
            .FirstOrDefault();

        if (dock is null)
        {
            return null;
        }

        return new SuggestionResponse
        {
            DockId = dock.Id,
            DockName = dock.Name,
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
