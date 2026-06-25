using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        if (!context.TerminalStates.Any())
        {
            context.TerminalStates.Add(new TerminalState
            {
                CurrentDay = 1,
                PlanningHorizon = 30
            });
            context.SaveChanges();
        }

        if (context.Docks.Any())
            return;

        // 8 docks: 1 XL, 1 L, 2 M, 4 S
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
        context.SaveChanges();

        // Ships: mix of Pending, Assigned, and Departed
        var ships = new List<Ship>
        {
            // Assigned (already docked)
            new Ship { Name = "Ocean Star", Size = ShipSize.XL, Status = ShipStatus.Assigned, ArrivalDay = 1, Duration = 10, Notes = "IMO: 9384756" },
            new Ship { Name = "Sea Queen", Size = ShipSize.M, Status = ShipStatus.Assigned, ArrivalDay = 1, Duration = 4, Notes = "IMO: 4419203" },
            new Ship { Name = "NF23", Size = ShipSize.S, Status = ShipStatus.Assigned, ArrivalDay = 1, Duration = 3, Notes = "IMO: NF23" },

            // Departed (completed occupation)
            new Ship { Name = "Amber Wave", Size = ShipSize.S, Status = ShipStatus.Departed, ArrivalDay = 1, Duration = 3, Notes = "IMO: 9374856" },
            new Ship { Name = "Silver Marine", Size = ShipSize.L, Status = ShipStatus.Departed, ArrivalDay = 1, Duration = 5, Notes = "IMO: 9218374" },

            // Pending (awaiting assignment)
            new Ship { Name = "Adriatic Pearl", Size = ShipSize.XL, Status = ShipStatus.Pending, ArrivalDay = 3, Duration = 7, Notes = "Priority cargo" },
            new Ship { Name = "North Wind", Size = ShipSize.L, Status = ShipStatus.Pending, ArrivalDay = 4, Duration = 5, Notes = "Late arrival" },
            new Ship { Name = "Blue Horizon", Size = ShipSize.M, Status = ShipStatus.Pending, ArrivalDay = 5, Duration = 4, Notes = "Refrigerated" },
            new Ship { Name = "Port Runner", Size = ShipSize.S, Status = ShipStatus.Pending, ArrivalDay = 3, Duration = 2, Notes = "Short stay" },
            new Ship { Name = "Harbor Line", Size = ShipSize.S, Status = ShipStatus.Pending, ArrivalDay = 6, Duration = 3, Notes = "" },
            new Ship { Name = "Silver Dock", Size = ShipSize.M, Status = ShipStatus.Pending, ArrivalDay = 7, Duration = 3, Notes = "" },
        };
        context.Ships.AddRange(ships);
        context.SaveChanges();

        // Assignments for the Assigned ships
        var assignments = new List<Assignment>
        {
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Ocean Star").Id,
                DockId = docks.Single(d => d.Name == "XL-01").Id,
                StartDay = 1,
                EndDay = 10
            },
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Sea Queen").Id,
                DockId = docks.Single(d => d.Name == "M-01").Id,
                StartDay = 1,
                EndDay = 4
            },
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "NF23").Id,
                DockId = docks.Single(d => d.Name == "S-03").Id,
                StartDay = 1,
                EndDay = 3
            }
        };
        context.Assignments.AddRange(assignments);
        context.SaveChanges();
    }
}
