using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class AssignmentService(AppDbContext db)
{
    public IReadOnlyList<Assignment> GetAll() => db.Assignments;

    public Assignment? GetById(int id) => db.Assignments.FirstOrDefault(a => a.Id == id);

    public Assignment Create(Assignment assignment)
    {
        assignment.Id = db.Assignments.Count == 0 ? 1 : db.Assignments.Max(existing => existing.Id) + 1;
        db.Assignments.Add(assignment);
        return assignment;
    }

    public Assignment? Update(int id, Action<Assignment> apply)
    {
        var assignment = GetById(id);
        if (assignment is null) return null;
        apply(assignment);
        return assignment;
    }

    public bool Delete(int id)
    {
        var assignment = GetById(id);
        if (assignment is null) return false;
        db.Assignments.Remove(assignment);
        return true;
    }
}
