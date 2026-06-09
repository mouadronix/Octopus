using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class DockService
{
    private readonly AppDbContext _context;

    public DockService(AppDbContext context)
    {
        _context = context;
    }

    public List<Dock> GetAll()
    {
        return _context.Docks.ToList();
    }

    public Dock? GetById(int id)
    {
        return _context.Docks.Find(id);
    }
}
