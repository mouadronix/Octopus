using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (!db.SystemStates.Any())
        {
            db.SystemStates.Add(new SystemState { Id = 1, CurrentDay = 1 });
        }

        if (db.Berths.Any())
        {
            db.SaveChanges();
            return;
        }

        db.Berths.AddRange([
            new Berth { Id = 1, Name = "Small Berth 1", Size = BerthSize.Small },
            new Berth { Id = 2, Name = "Small Berth 2", Size = BerthSize.Small },
            new Berth { Id = 3, Name = "Medium Berth 1", Size = BerthSize.Medium },
            new Berth { Id = 4, Name = "Medium Berth 2", Size = BerthSize.Medium },
            new Berth { Id = 5, Name = "Large Berth 1", Size = BerthSize.Large }
        ]);

        db.SaveChanges();
    }
}
