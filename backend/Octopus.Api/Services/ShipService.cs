using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class ShipService(AppDbContext db, ILogger<ShipService> logger) : IShipService
{
    public async Task<IReadOnlyList<Ship>> GetAllAsync(CancellationToken ct = default)
        => await db.Ships.AsNoTracking().ToListAsync(ct);

    public async Task<Ship?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Ships.FindAsync([id], ct);

    public async Task<Ship> CreateAsync(Ship ship, CancellationToken ct = default)
    {
        db.Ships.Add(ship);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created ship {ShipId} ({Name})", ship.Id, ship.Name);
        return ship;
    }

    public async Task<Ship?> UpdateAsync(int id, Action<Ship> apply, CancellationToken ct = default)
    {
        var ship = await db.Ships.FindAsync([id], ct);
        if (ship is null) return null;
        apply(ship);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated ship {ShipId}", id);
        return ship;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var ship = await db.Ships.FindAsync([id], ct);
        if (ship is null) return false;
        db.Ships.Remove(ship);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted ship {ShipId}", id);
        return true;
    }
}
