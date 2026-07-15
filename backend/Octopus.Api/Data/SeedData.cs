using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Skip if data already seeded
        if (context.Docks.Any())
            return;

        var terminalState = new TerminalState
        {
            CurrentDay = 12,
            PlanningHorizon = 30
        };
        context.TerminalStates.Add(terminalState);

        // Spec: 8 docks — 1 XL, 1 L, 2 M, 4 S
        var docks = new List<Dock>
        {
            new Dock { Name = "XL-01", Size = ShipSize.XL },
            new Dock { Name = "L-01",  Size = ShipSize.L  },
            new Dock { Name = "M-01",  Size = ShipSize.M  },
            new Dock { Name = "M-02",  Size = ShipSize.M  },
            new Dock { Name = "S-01",  Size = ShipSize.S  },
            new Dock { Name = "S-02",  Size = ShipSize.S  },
            new Dock { Name = "S-03",  Size = ShipSize.S  },
            new Dock { Name = "S-04",  Size = ShipSize.S  },
        };
        context.Docks.AddRange(docks);

        var ships = new List<Ship>
        {
            new Ship { Name = "Ocean Star", Size = ShipSize.XL, Status = ShipStatus.Assigned, ArrivalDay = 5, Duration = 10, Notes = "IMO: 9384756" },
            new Ship { Name = "Sea Queen", Size = ShipSize.M, Status = ShipStatus.Assigned, ArrivalDay = 7, Duration = 4, Notes = "IMO: 4419203" },
            new Ship { Name = "NF23", Size = ShipSize.S, Status = ShipStatus.Assigned, ArrivalDay = 9, Duration = 3, Notes = "IMO: NF23" },
            new Ship { Name = "Adriatic Pearl", Size = ShipSize.XL, Status = ShipStatus.Pending, ArrivalDay = 13, Duration = 7, Notes = "Priority cargo" },
            new Ship { Name = "North Wind", Size = ShipSize.L, Status = ShipStatus.Pending, ArrivalDay = 14, Duration = 5, Notes = "Late arrival" },
            new Ship { Name = "Blue Horizon", Size = ShipSize.M, Status = ShipStatus.Pending, ArrivalDay = 15, Duration = 4, Notes = "Refrigerated" },
            new Ship { Name = "Port Runner", Size = ShipSize.S, Status = ShipStatus.Pending, ArrivalDay = 13, Duration = 2, Notes = "Short stay" },
            new Ship { Name = "Harbor Line", Size = ShipSize.S, Status = ShipStatus.Pending, ArrivalDay = 16, Duration = 3, Notes = "" },
            new Ship { Name = "Silver Dock", Size = ShipSize.M, Status = ShipStatus.Pending, ArrivalDay = 17, Duration = 3, Notes = "" },
            new Ship { Name = "Pacific Trader", Size = ShipSize.XL, Status = ShipStatus.Assigned, ArrivalDay = 3, Duration = 12, Notes = "IMO: 9102837" },
            new Ship { Name = "Coastal Express", Size = ShipSize.M, Status = ShipStatus.Assigned, ArrivalDay = 10, Duration = 6, Notes = "IMO: 9283746" },
            new Ship { Name = "Amber Wave", Size = ShipSize.S, Status = ShipStatus.Departed, ArrivalDay = 2, Duration = 3, Notes = "IMO: 9374856" },
            new Ship { Name = "Silver Marine", Size = ShipSize.L, Status = ShipStatus.Departed, ArrivalDay = 1, Duration = 7, Notes = "IMO: 9218374" },
            new Ship { Name = "Atlantic Crown", Size = ShipSize.XL, Status = ShipStatus.Assigned, ArrivalDay = 4, Duration = 9, Notes = "IMO: 9044551" },
            new Ship { Name = "Red Harbor", Size = ShipSize.L, Status = ShipStatus.Assigned, ArrivalDay = 6, Duration = 5, Notes = "IMO: 9098123" },
            new Ship { Name = "Ionian Star", Size = ShipSize.M, Status = ShipStatus.Assigned, ArrivalDay = 8, Duration = 6, Notes = "IMO: 9307114" },
            new Ship { Name = "Metro Cargo", Size = ShipSize.S, Status = ShipStatus.Assigned, ArrivalDay = 11, Duration = 2, Notes = "IMO: 9114007" },
            new Ship { Name = "Blue Harbor", Size = ShipSize.L, Status = ShipStatus.Departed, ArrivalDay = 12, Duration = 4, Notes = "IMO: 9452220" },
        };
        context.Ships.AddRange(ships);
        context.SaveChanges();

        var assignments = new List<Assignment>
        {
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Ocean Star").Id,
                DockId = docks.Single(d => d.Name == "XL-01").Id,
                StartDay = 5,
                EndDay = 15
            },
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Sea Queen").Id,
                DockId = docks.Single(d => d.Name == "M-01").Id,
                StartDay = 7,
                EndDay = 11
            },
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "NF23").Id,
                DockId = docks.Single(d => d.Name == "S-03").Id,
                StartDay = 9,
                EndDay = 12
            }
        };

        context.Assignments.AddRange(assignments);
        context.SaveChanges();
    }
}
