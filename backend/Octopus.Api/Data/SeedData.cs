using Octopus.Api.Models;

namespace Octopus.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Ensure terminal state exists
        if (!context.TerminalStates.Any())
        {
            context.TerminalStates.Add(new TerminalState
            {
                CurrentDay = 1,
                PlanningHorizon = 30
            });
            context.SaveChanges();
        }

        // Skip if docks already seeded
        if (context.Docks.Any())
            return;

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

        // ────────────────────────────────────────────────────────────
        // Full month of port traffic — 39 ships across 30 days
        // ────────────────────────────────────────────────────────────
        var ships = new List<Ship>
        {
            // ── XL-01 schedule ──────────────────────────────────────
            new Ship { Name = "Ocean Monarch",    Size = ShipSize.XL, Status = ShipStatus.Pending, ArrivalDay = 2,  Duration = 8,  Notes = "Container vessel" },
            new Ship { Name = "Pacific Titan",    Size = ShipSize.XL, Status = ShipStatus.Pending, ArrivalDay = 10, Duration = 10, Notes = "Bulk carrier" },
            new Ship { Name = "Nordic Voyager",   Size = ShipSize.XL, Status = ShipStatus.Pending, ArrivalDay = 22, Duration = 7,  Notes = "Ro-Ro" },

            // ── L-01 schedule ───────────────────────────────────────
            new Ship { Name = "Sea Navigator",    Size = ShipSize.L,  Status = ShipStatus.Pending, ArrivalDay = 1,  Duration = 5,  Notes = "General cargo" },
            new Ship { Name = "Atlantic Pride",   Size = ShipSize.L,  Status = ShipStatus.Pending, ArrivalDay = 6,  Duration = 8,  Notes = "Tanker" },
            new Ship { Name = "Gulf Runner",      Size = ShipSize.L,  Status = ShipStatus.Pending, ArrivalDay = 16, Duration = 5,  Notes = "Reefer" },
            new Ship { Name = "Caribbean Breeze",  Size = ShipSize.L,  Status = ShipStatus.Pending, ArrivalDay = 24, Duration = 4,  Notes = "Passenger" },

            // ── M-01 schedule ───────────────────────────────────────
            new Ship { Name = "Coral Bay",        Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 1,  Duration = 4,  Notes = "Feeder" },
            new Ship { Name = "Marina Star",      Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 5,  Duration = 6,  Notes = "Bulk carrier" },
            new Ship { Name = "Jade Wave",        Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 13, Duration = 5,  Notes = "Container" },
            new Ship { Name = "Sapphire Coast",   Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 20, Duration = 4,  Notes = "General cargo" },

            // ── M-02 schedule ───────────────────────────────────────
            new Ship { Name = "Iron Cape",        Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 3,  Duration = 5,  Notes = "Multi-purpose" },
            new Ship { Name = "Bronze Eagle",     Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 8,  Duration = 7,  Notes = "Heavy lift" },
            new Ship { Name = "Emerald Isle",     Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 17, Duration = 3,  Notes = "Feeder" },
            new Ship { Name = "Ruby Harbor",      Size = ShipSize.M,  Status = ShipStatus.Pending, ArrivalDay = 23, Duration = 5,  Notes = "Tanker" },

            // ── S-01 schedule ───────────────────────────────────────
            new Ship { Name = "Swift Arrow",      Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 1,  Duration = 3,  Notes = "Tug supply" },
            new Ship { Name = "Delta Fox",        Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 4,  Duration = 3,  Notes = "Pilot boat" },
            new Ship { Name = "Vega Star",        Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 8,  Duration = 4,  Notes = "Research" },
            new Ship { Name = "Orion Mist",       Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 14, Duration = 3,  Notes = "Patrol" },
            new Ship { Name = "Pegasus Wing",     Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 20, Duration = 2,  Notes = "Fast supply" },
            new Ship { Name = "Nova Light",       Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 25, Duration = 3,  Notes = "Tug" },

            // ── S-02 schedule ───────────────────────────────────────
            new Ship { Name = "Sparrow",          Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 1,  Duration = 2,  Notes = "Pilot" },
            new Ship { Name = "Kestrel",          Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 4,  Duration = 3,  Notes = "Supply" },
            new Ship { Name = "Falcon",           Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 8,  Duration = 3,  Notes = "Patrol" },
            new Ship { Name = "Hawk",             Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 13, Duration = 2,  Notes = "Fast crew" },
            new Ship { Name = "Eagle",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 18, Duration = 3,  Notes = "Tug" },
            new Ship { Name = "Osprey",           Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 24, Duration = 2,  Notes = "Pilot" },

            // ── S-03 schedule ───────────────────────────────────────
            new Ship { Name = "Tide",             Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 1,  Duration = 2,  Notes = "Supply" },
            new Ship { Name = "Wave",             Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 3,  Duration = 3,  Notes = "Research" },
            new Ship { Name = "Surge",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 7,  Duration = 4,  Notes = "Tug supply" },
            new Ship { Name = "Ripple",           Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 14, Duration = 2,  Notes = "Patrol" },
            new Ship { Name = "Current",          Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 19, Duration = 3,  Notes = "Pilot" },
            new Ship { Name = "Drift",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 25, Duration = 3,  Notes = "Supply" },

            // ── S-04 schedule ───────────────────────────────────────
            new Ship { Name = "Ember",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 2,  Duration = 2,  Notes = "Tug" },
            new Ship { Name = "Flame",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 5,  Duration = 3,  Notes = "Fast crew" },
            new Ship { Name = "Spark",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 9,  Duration = 2,  Notes = "Pilot" },
            new Ship { Name = "Blaze",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 13, Duration = 3,  Notes = "Patrol" },
            new Ship { Name = "Torch",            Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 19, Duration = 2,  Notes = "Supply" },
            new Ship { Name = "Beacon",           Size = ShipSize.S,  Status = ShipStatus.Pending, ArrivalDay = 24, Duration = 3,  Notes = "Tug" },
        };
        context.Ships.AddRange(ships);
        context.SaveChanges();

        // Assign currently-docked ships (arrival <= currentDay, arrival+duration > currentDay)
        // CurrentDay = 1, so ships arriving on day 1 are docked
        var assignments = new List<Assignment>
        {
            // Sea Navigator on L-01 (days 1–5)
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Sea Navigator").Id,
                DockId = docks.Single(d => d.Name == "L-01").Id,
                StartDay = 1,
                EndDay = 5
            },
            // Swift Arrow on S-01 (days 1–3)
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Swift Arrow").Id,
                DockId = docks.Single(d => d.Name == "S-01").Id,
                StartDay = 1,
                EndDay = 3
            },
            // Tide on S-03 (days 1–2)
            new Assignment
            {
                ShipId = ships.Single(s => s.Name == "Tide").Id,
                DockId = docks.Single(d => d.Name == "S-03").Id,
                StartDay = 1,
                EndDay = 2
            },
        };

        // Mark assigned ships
        foreach (var a in assignments)
        {
            ships.Single(s => s.Id == a.ShipId).Status = ShipStatus.Assigned;
        }

        context.Assignments.AddRange(assignments);
        context.SaveChanges();
    }
}
