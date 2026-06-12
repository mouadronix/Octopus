using Octopus.Api.Models;
using Octopus.Api.Services;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        var shouldRefreshDemoData =
            !context.Assignments.Any() ||
            !context.Docks.Any(d => d.Name == "XL-02") ||
            !context.Ships.Any(s => s.Name == "Ocean Star");

        if (!shouldRefreshDemoData)
        {
            SeedUsers(context);
            EnsureShipCatalog(context);
            return;
        }

        context.Assignments.RemoveRange(context.Assignments);
        context.Ships.RemoveRange(context.Ships);
        context.Docks.RemoveRange(context.Docks);
        context.TerminalStates.RemoveRange(context.TerminalStates);
        context.SaveChanges();

        var terminalState = new TerminalState
        {
            CurrentDay = 12,
            PlanningHorizon = 30
        };
        context.TerminalStates.Add(terminalState);

        var docks = new List<Dock>
        {
            new Dock { Name = "XL-01", Size = ShipSize.XL },
            new Dock { Name = "XL-02", Size = ShipSize.XL },
            new Dock { Name = "L-01",  Size = ShipSize.L  },
            new Dock { Name = "L-02",  Size = ShipSize.L  },
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

        SeedUsers(context);
        EnsureShipCatalog(context);
    }

    private static void EnsureShipCatalog(AppDbContext context)
    {
        var ships = new List<Ship>
        {
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

        foreach (var ship in ships)
        {
            var existing = context.Ships.FirstOrDefault(existingShip => existingShip.Name == ship.Name);
            if (existing is null)
            {
                context.Ships.Add(ship);
                continue;
            }

            existing.Size = ship.Size;
            existing.Status = ship.Status;
            existing.ArrivalDay = ship.ArrivalDay;
            existing.Duration = ship.Duration;
            existing.Notes = ship.Notes;
        }

        context.SaveChanges();
    }

    private static void SeedUsers(AppDbContext context)
    {
        if (!context.Users.Any(u => u.Username == "admin"))
        {
            context.Users.Add(new AppUser
            {
                FullName = "Administrator",
                Username = "admin",
                PasswordHash = AuthService.HashPassword("admin"),
                Role = "Scheduler"
            });
        }

        if (!context.Users.Any(u => u.Username == "guest"))
        {
            context.Users.Add(new AppUser
            {
                FullName = "Guest Operator",
                Username = "guest",
                PasswordHash = AuthService.HashPassword("guest"),
                Role = "Guest"
            });
        }

        context.SaveChanges();
    }
}
