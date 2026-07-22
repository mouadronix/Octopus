using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Tests.Helpers;

public static class SeedHelper
{
    public static void SeedBasicData(AppDbContext ctx)
    {
        ctx.Docks.AddRange(
            new Dock { Id = 1, Name = "XL-01", Size = ShipSize.XL },
            new Dock { Id = 2, Name = "L-01", Size = ShipSize.L },
            new Dock { Id = 3, Name = "M-01", Size = ShipSize.M },
            new Dock { Id = 4, Name = "S-01", Size = ShipSize.S }
        );

        ctx.TerminalStates.Add(
            new TerminalState { Id = 1, CurrentDay = 1, PlanningHorizon = 60 }
        );

        ctx.SaveChanges();
    }
}
