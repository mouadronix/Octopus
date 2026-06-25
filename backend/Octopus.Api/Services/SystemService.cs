using Microsoft.EntityFrameworkCore;
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

    public SystemState GetState()
    {
        var terminal = _context.TerminalStates.FirstOrDefault();

        return new SystemState
        {
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            ServerTimeUtc = DateTime.UtcNow,
            CurrentDay = terminal?.CurrentDay ?? 1,
            ShipCount = _context.Ships.Count(),
            BerthCount = _context.Docks.Count(),
            ActiveAssignmentCount = _context.Assignments.Count()
        };
    }

    public SystemState AdvanceDay()
    {
        var state = _context.TerminalStates.FirstOrDefault();
        if (state is null)
        {
            state = new TerminalState { CurrentDay = 1, PlanningHorizon = 30 };
            _context.TerminalStates.Add(state);
        }

        state.CurrentDay++;

        var assignedShips = _context.Ships
            .Include(s => s.Assignment)
            .Where(s => s.Status == ShipStatus.Assigned)
            .ToList();

        foreach (var ship in assignedShips)
        {
            if (ship.Assignment is not null && ship.Assignment.EndDay < state.CurrentDay)
            {
                ship.Status = ShipStatus.Departed;
            }
        }

        _context.SaveChanges();
        return GetState();
    }
}
