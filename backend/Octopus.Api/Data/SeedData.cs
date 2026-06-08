using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Ships.Count > 0 || db.Berths.Count > 0)
        {
            return;
        }

        db.Ships.AddRange(
        [
            new Ship
            {
                Id = 1,
                Name = "Aegean Star",
                ImoNumber = "IMO-1000001",
                CargoType = "Containers",
                EstimatedArrival = DateTime.UtcNow.AddHours(6)
            },
            new Ship
            {
                Id = 2,
                Name = "Harbor Light",
                ImoNumber = "IMO-1000002",
                CargoType = "Bulk",
                EstimatedArrival = DateTime.UtcNow.AddHours(12)
            }
        ]);

        db.Berths.AddRange(
        [
            new Berth { Id = 1, Name = "Berth A", MaxDraftMeters = 14.5m },
            new Berth { Id = 2, Name = "Berth B", MaxDraftMeters = 11.0m }
        ]);

        db.Assignments.Add(new Assignment
        {
            Id = 1,
            ShipId = 1,
            BerthId = 1,
            StartsAt = DateTime.UtcNow.AddHours(7),
            Status = "Planned"
        });
    }
}
