using Microsoft.EntityFrameworkCore;
using Octopus.Api.Common;
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
        if (ship.ArrivalDay < terminal.CurrentDay || !CanFitShip(dock.Size, ship.Size))
        {
            return (false, -1);
        }

        var assignments = _context.Assignments
            .Where(a => a.DockId == dock.Id)
            .OrderBy(a => a.StartDay)
            .ToList();

        var candidate = Math.Max(ship.ArrivalDay, terminal.CurrentDay);
        var maxDay = terminal.CurrentDay + terminal.PlanningHorizon;

        foreach (var assignment in assignments)
        {
            var candidateEnd = candidate + ship.Duration - 1;
            if (candidateEnd < assignment.StartDay)
            {
                return (true, candidate);
            }

            candidate = assignment.EndDay + 1;
        }

        return candidate + ship.Duration - 1 <= maxDay
            ? (true, candidate)
            : (false, -1);
    }

    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        if (ship == null || ship.Status != ShipStatus.Pending)
        {
            return null;
        }

        var terminal = GetOrCreateTerminalState();

        var best = _context.Docks
            .AsEnumerable()
            .Where(dock => CanFitShip(dock.Size, ship.Size))
            .Select(dock => new
            {
                Dock = dock,
                Result = CanAssign(ship, dock, terminal)
            })
            .Where(item => item.Result.CanAssign)
            .OrderBy(item => item.Result.StartDay)
            .ThenBy(item => SizeRank(item.Dock.Size))
            .ThenBy(item => item.Dock.Name)
            .FirstOrDefault();

        if (best == null)
        {
            return null;
        }

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

    public Result<Assignment> AssignShip(int shipId, int dockId)
    {
        var ship = _context.Ships
            .Include(s => s.Assignment)
            .FirstOrDefault(s => s.Id == shipId);
        if (ship == null)
        {
            return Result<Assignment>.Fail("SHIP_NOT_FOUND", "Ship not found");
        }

        if (ship.Status != ShipStatus.Pending || ship.Assignment != null)
        {
            return Result<Assignment>.Fail("SHIP_NOT_PENDING", "Ship is not pending");
        }

        var dock = _context.Docks.Find(dockId);
        if (dock == null)
        {
            return Result<Assignment>.Fail("DOCK_NOT_FOUND", "Dock not found");
        }

        if (!CanFitShip(dock.Size, ship.Size))
        {
            return Result<Assignment>.Fail("SIZE_MISMATCH", "Dock size does not fit ship size");
        }

        var terminal = GetOrCreateTerminalState();
        var (canAssign, startDay) = CanAssign(ship, dock, terminal);
        if (!canAssign)
        {
            return Result<Assignment>.Fail("NO_SLOT", "No available slot for this ship");
        }

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

    private TerminalState GetOrCreateTerminalState()
    {
        var terminal = _context.TerminalStates.FirstOrDefault();
        if (terminal != null)
        {
            return terminal;
        }

        terminal = new TerminalState { CurrentDay = 1, PlanningHorizon = 30 };
        _context.TerminalStates.Add(terminal);
        _context.SaveChanges();
        return terminal;
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
