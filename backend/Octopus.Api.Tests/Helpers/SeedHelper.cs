using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Tests.Helpers;

public static class SeedHelper
{
    public static void SeedBasicData(AppDbContext ctx)
    {
        ctx.Docks.AddRange(
            new Dock { Id = 1, Name = "Dock A", Size = ShipSize.L },
            new Dock { Id = 2, Name = "Dock B", Size = ShipSize.M },
            new Dock { Id = 3, Name = "Dock C", Size = ShipSize.S }
        );

        ctx.TerminalStates.Add(
            new TerminalState { Id = 1, CurrentDay = 1, PlanningHorizon = 30 }
        );

        ctx.SaveChanges();
    }
}
