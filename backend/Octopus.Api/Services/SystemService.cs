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

    public SystemState GetState()
    {
        var terminal = _context.TerminalStates.FirstOrDefault();
        return new SystemState
        {
            Environment = "Development",
            ServerTimeUtc = DateTime.UtcNow,
            ShipCount = _context.Ships.Count(),
            BerthCount = _context.Docks.Count(),
            ActiveAssignmentCount = _context.Assignments.Count()
        };
    }
}
