using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class ShipService
{
    private readonly AppDbContext _context;

    public ShipService(AppDbContext context)
    {
        _context = context;
    }

    public List<Ship> GetAll()
    {
        return _context.Ships.ToList();
    }
}
