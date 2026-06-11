using Octopus.Api.Data;
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
        return _context.Assignments.ToList();
    }

    public Dock? GetSuggestion(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        if (ship == null) return null;

        // Find a dock that matches the ship's size and has no conflicting assignments
        return _context.Docks
            .Where(d => d.Size == ship.Size)
            .AsEnumerable()
            .FirstOrDefault(d => !_context.Assignments.Any(a =>
                a.DockId == d.Id &&
                a.StartDay < ship.ArrivalDay + ship.Duration &&
                a.EndDay > ship.ArrivalDay));
    }
}
