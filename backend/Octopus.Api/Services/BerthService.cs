using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class BerthService(AppDbContext db, ILogger<BerthService> logger) : IBerthService
{
    public async Task<IReadOnlyList<Berth>> GetAllAsync(CancellationToken ct = default)
        => await db.Berths.AsNoTracking().ToListAsync(ct);

    public async Task<Berth?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Berths.FindAsync([id], ct);

    public async Task<Berth> CreateAsync(Berth berth, CancellationToken ct = default)
    {
        db.Berths.Add(berth);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created berth {BerthId} ({Name})", berth.Id, berth.Name);
        return berth;
    }

    public async Task<Berth?> UpdateAsync(int id, Action<Berth> apply, CancellationToken ct = default)
    {
        var berth = await db.Berths.FindAsync([id], ct);
        if (berth is null) return null;
        apply(berth);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated berth {BerthId}", id);
        return berth;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var berth = await db.Berths.FindAsync([id], ct);
        if (berth is null) return false;
        db.Berths.Remove(berth);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted berth {BerthId}", id);
        return true;
    }
}
