using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class SystemService
{
    private readonly AppDbContext _context;

    public SystemService(AppDbContext context)
    {
        _context = context;
    }

    public int GetCurrentDay()
    {
        return GetOrCreateTerminalState().CurrentDay;
    }

    public SystemState GetState()
    {
        var terminal = GetOrCreateTerminalState();

        return new SystemState
        {
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            ServerTimeUtc = DateTime.UtcNow,
            CurrentDay = terminal.CurrentDay,
            ShipCount = _context.Ships.Count(),
            BerthCount = _context.Docks.Count(),
            ActiveAssignmentCount = _context.Assignments.Count(a => a.EndDay >= terminal.CurrentDay)
        };
    }

    public SystemState AdvanceDay()
    {
        var terminal = GetOrCreateTerminalState();
        terminal.CurrentDay += 1;

        var assignedShips = _context.Ships
            .Where(s => s.Status == ShipStatus.Assigned)
            .ToList();

        foreach (var ship in assignedShips)
        {
            var assignment = _context.Assignments.FirstOrDefault(a => a.ShipId == ship.Id);
            if (assignment != null && assignment.EndDay < terminal.CurrentDay)
            {
                ship.Status = ShipStatus.Departed;
            }
        }

        _context.SaveChanges();
        return GetState();
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
}
