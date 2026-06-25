using Microsoft.EntityFrameworkCore;
using Octopus.Api.Models;

namespace Octopus.Api.Data;

/// <summary>
/// EF Core implementation of IAssignmentRepository.
/// </summary>
public class EfAssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public EfAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public AssignmentContext? GetAssignmentContext(int shipId, int dockId)
    {
        var ship = _context.Ships
            .Include(s => s.Assignment)
            .FirstOrDefault(s => s.Id == shipId);

        var dock = _context.Docks.Find(dockId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || dock is null || terminal is null)
            return null;

        var dockAssignments = _context.Assignments
            .Where(a => a.DockId == dock.Id)
            .ToList();

        return new AssignmentContext(ship, dock, terminal, dockAssignments);
    }

    public SuggestionContext? GetSuggestionContext(int shipId)
    {
        var ship = _context.Ships.Find(shipId);
        var terminal = _context.TerminalStates.FirstOrDefault();

        if (ship is null || terminal is null)
            return null;

        var allDocks = _context.Docks.ToList();

        var assignmentsByDock = _context.Assignments
            .GroupBy(a => a.DockId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new SuggestionContext(ship, terminal, allDocks, assignmentsByDock);
    }

    public List<Assignment> GetAllWithDetails()
    {
        return _context.Assignments
            .Include(a => a.Ship)
            .Include(a => a.Dock)
            .OrderBy(a => a.StartDay)
            .ToList();
    }

    public void Add(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
