using Microsoft.EntityFrameworkCore;
using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Ships.AnyAsync())
            return;

        db.Ships.AddRange(
            new Ship
            {
                Name = "Aegean Star",
                ImoNumber = "IMO-1000001",
                CargoType = "Containers",
                EstimatedArrival = DateTime.UtcNow.AddHours(6)
            },
            new Ship
            {
                Name = "Harbor Light",
                ImoNumber = "IMO-1000002",
                CargoType = "Bulk",
                EstimatedArrival = DateTime.UtcNow.AddHours(12)
            });

        db.Berths.AddRange(
            new Berth { Name = "Berth A", MaxDraftMeters = 14.5m },
            new Berth { Name = "Berth B", MaxDraftMeters = 11.0m });

        await db.SaveChangesAsync();

        db.Assignments.Add(new Assignment
        {
            ShipId = 1,
            BerthId = 1,
            StartsAt = DateTime.UtcNow.AddHours(7),
            Status = "Planned"
        });

        await db.SaveChangesAsync();
    }
}
