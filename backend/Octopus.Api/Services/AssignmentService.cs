using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class AssignmentService(AppDbContext db, ILogger<AssignmentService> logger) : IAssignmentService
{
    public async Task<IReadOnlyList<Assignment>> GetAllAsync(CancellationToken ct = default)
        => await db.Assignments.AsNoTracking().ToListAsync(ct);

    public async Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Assignments.FindAsync([id], ct);

    public async Task<Assignment> CreateAsync(Assignment assignment, CancellationToken ct = default)
    {
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created assignment {AssignmentId} (Ship {ShipId} -> Berth {BerthId})",
            assignment.Id, assignment.ShipId, assignment.BerthId);
        return assignment;
    }

    public async Task<Assignment?> UpdateAsync(int id, Action<Assignment> apply, CancellationToken ct = default)
    {
        var assignment = await db.Assignments.FindAsync([id], ct);
        if (assignment is null) return null;
        apply(assignment);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated assignment {AssignmentId}", id);
        return assignment;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var assignment = await db.Assignments.FindAsync([id], ct);
        if (assignment is null) return false;
        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted assignment {AssignmentId}", id);
        return true;
    }
}
