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

    public TerminalState? GetState()
    {
        return _context.TerminalStates.FirstOrDefault();
    }
}
