using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class AssignmentService(AppDbContext db)
{
    public IReadOnlyList<Assignment> GetAll() => db.Assignments;

    public Assignment Create(Assignment assignment)
    {
        assignment.Id = db.Assignments.Count == 0 ? 1 : db.Assignments.Max(existing => existing.Id) + 1;
        db.Assignments.Add(assignment);
        return assignment;
    }
}
