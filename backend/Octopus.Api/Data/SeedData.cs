using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Already seeded
        if (context.Docks.Any()) return;

        // Seed TerminalState
        var terminalState = new TerminalState
        {
            CurrentDay = 1,
            PlanningHorizon = 30
        };
        context.TerminalStates.Add(terminalState);

        // Seed Docks: 1 XL, 1 L, 2 M, 4 S
        var docks = new List<Dock>
        {
            new Dock { Name = "Dock XL-1", Size = ShipSize.XL },
            new Dock { Name = "Dock L-1",  Size = ShipSize.L  },
            new Dock { Name = "Dock M-1",  Size = ShipSize.M  },
            new Dock { Name = "Dock M-2",  Size = ShipSize.M  },
            new Dock { Name = "Dock S-1",  Size = ShipSize.S  },
            new Dock { Name = "Dock S-2",  Size = ShipSize.S  },
            new Dock { Name = "Dock S-3",  Size = ShipSize.S  },
            new Dock { Name = "Dock S-4",  Size = ShipSize.S  },
        };
        context.Docks.AddRange(docks);

        // Seed sample Ships
        var ships = new List<Ship>
        {
            new Ship { Name = "MSC Aurora",    Size = ShipSize.XL, Status = ShipStatus.Pending,  ArrivalDay = 3,  Duration = 10, Notes = "Priority cargo" },
            new Ship { Name = "Costa Marina",  Size = ShipSize.L,  Status = ShipStatus.Pending,  ArrivalDay = 5,  Duration = 7,  Notes = "" },
            new Ship { Name = "Ever Glory",    Size = ShipSize.M,  Status = ShipStatus.Pending,  ArrivalDay = 2,  Duration = 6,  Notes = "Refrigerated" },
            new Ship { Name = "Pacific Star",  Size = ShipSize.S,  Status = ShipStatus.Assigned, ArrivalDay = 1,  Duration = 4,  Notes = "" },
            new Ship { Name = "MSC Helena",    Size = ShipSize.M,  Status = ShipStatus.Departed, ArrivalDay = 1,  Duration = 5,  Notes = "" },
        };
        context.Ships.AddRange(ships);

        context.SaveChanges();
    }
}
