using Octopus.Api.Data;
using Octopus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Octopus.Api.Services;

public class DockService
{
    private readonly AppDbContext _context;

    public DockService(AppDbContext context)
    {
        _context = context;
    }


// Returns all docks
    public List<Dock> GetAll()
    {
        return _context.Docks
            .Include(d => d.Assignments)
            .ThenInclude(a => a.Ship)
            .OrderBy(d => d.Size)
            .ThenBy(d => d.Name)
            .ToList();
    }

    public Dock? GetById(int id)
    {
        return _context.Docks.Find(id);
    }
}
