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
        return _context.TerminalStates.First().CurrentDay;
    }

    public int AdvanceDay()
    {
        var state = _context.TerminalStates.First();
        state.CurrentDay++;

        var assignedShips = _context.Ships
            .Where(s => s.Status == ShipStatus.Assigned)
            .ToList();

        foreach (var ship in assignedShips)
        {
            var assignment = _context.Assignments.FirstOrDefault(a => a.ShipId == ship.Id);
            if (assignment != null && assignment.EndDay < state.CurrentDay)
            {
                ship.Status = ShipStatus.Departed;
            }
        }

        _context.SaveChanges();
        return state.CurrentDay;
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
