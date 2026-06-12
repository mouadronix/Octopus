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



// Returns the current system state and statistics
    public SystemState GetState()
    {
        var terminal = _context.TerminalStates.FirstOrDefault();
        return new SystemState
        {
            Environment = "Development",
            ServerTimeUtc = DateTime.UtcNow,
            CurrentDay = terminal?.CurrentDay ?? 1,
            ShipCount = _context.Ships.Count(),
            BerthCount = _context.Docks.Count(),
            ActiveAssignmentCount = _context.Assignments.Count()
        };
    }

    public SystemState AdvanceDay()
    {
        var terminal = _context.TerminalStates.FirstOrDefault();
        if (terminal == null)
        {
            terminal = new TerminalState { CurrentDay = 1, PlanningHorizon = 30 };
            _context.TerminalStates.Add(terminal);
        }

        terminal.CurrentDay += 1;
        _context.SaveChanges();
        return GetState();
    }
}
