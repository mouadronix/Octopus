using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class AssignmentService(AppDbContext db)
{
    public IReadOnlyList<Assignment> GetAll() =>
        db.Assignments
            .Include(assignment => assignment.Ship)
            .Include(assignment => assignment.Berth)
            .OrderBy(assignment => assignment.StartDay)
            .AsNoTracking()
            .ToList();

    public Assignment Create(Assignment assignment)
    {
        db.Assignments.Add(assignment);
        db.SaveChanges();
        return assignment;
    }
}
