using System.Linq;
using Xunit;
using Octopus.Api.Models;
using Octopus.Api.Services;
using Octopus.Api.Tests.Helpers;

namespace Octopus.Api.Tests.Services;

public class DockServiceTests
{
    // ---------------------------------------------------------------
    // 1. GetAll_ShouldReturnAllDocks
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldReturnAllDocks()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        SeedHelper.SeedBasicData(context);
        var service = new DockService(context);

        var all = service.GetAll();

        Assert.Equal(3, all.Count);
    }

    // ---------------------------------------------------------------
    // 2. GetAll_ShouldIncludeAssignments
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldIncludeAssignments()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        SeedHelper.SeedBasicData(context);
        var service = new DockService(context);

        // Add a ship and an assignment linked to dock 1
        var ship = new Ship { Name = "TestShip", Size = ShipSize.M, ArrivalDay = 1, Duration = 5 };
        context.Ships.Add(ship);
        context.SaveChanges();

        context.Assignments.Add(new Assignment
        {
            ShipId = ship.Id,
            DockId = 1,
            StartDay = 1,
            EndDay = 5
        });
        context.SaveChanges();

        var all = service.GetAll();

        var dock1 = all.First(d => d.Id == 1);
        Assert.Single(dock1.Assignments);
        Assert.NotNull(dock1.Assignments.First().Ship);
        Assert.Equal("TestShip", dock1.Assignments.First().Ship.Name);
    }

    // ---------------------------------------------------------------
    // 3. GetAll_ShouldOrderBySizeThenName
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldOrderBySizeThenName()
    {
        using var context = TestDbContextFactory.CreateDbContext();

        // Insert in reverse expected order to verify sorting
        context.Docks.AddRange(
            new Dock { Id = 1, Name = "Zulu", Size = ShipSize.S },
            new Dock { Id = 2, Name = "Alpha", Size = ShipSize.S },
            new Dock { Id = 3, Name = "Bravo", Size = ShipSize.L },
            new Dock { Id = 4, Name = "Delta", Size = ShipSize.M }
        );
        context.SaveChanges();

        var service = new DockService(context);
        var all = service.GetAll();

        // ShipSize enum: XL=0, L=1, M=2, S=3
        // Expected order: L(Bravo), M(Delta), S(Alpha), S(Zulu)
        Assert.Equal(4, all.Count);
        Assert.Equal("Bravo", all[0].Name);  // L
        Assert.Equal("Delta", all[1].Name);  // M
        Assert.Equal("Alpha", all[2].Name);  // S (A < Z)
        Assert.Equal("Zulu", all[3].Name);   // S
    }

    // ---------------------------------------------------------------
    // 4. GetById_ExistingId_ShouldReturnDock
    // ---------------------------------------------------------------
    [Fact]
    public void GetById_ExistingId_ShouldReturnDock()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        SeedHelper.SeedBasicData(context);
        var service = new DockService(context);

        var found = service.GetById(1);

        Assert.NotNull(found);
        Assert.Equal("Dock A", found.Name);
        Assert.Equal(ShipSize.L, found.Size);
    }

    // ---------------------------------------------------------------
    // 5. GetById_NonExistingId_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void GetById_NonExistingId_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        SeedHelper.SeedBasicData(context);
        var service = new DockService(context);

        var found = service.GetById(999);

        Assert.Null(found);
    }
}
