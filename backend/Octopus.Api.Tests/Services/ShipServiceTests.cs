using Xunit;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;
using Octopus.Api.Tests.Helpers;

namespace Octopus.Api.Tests.Services;

public class ShipServiceTests
{
    private static CreateShipRequest MakeRequest(
        string name = "TestShip",
        ShipSize size = ShipSize.M,
        int arrivalDay = 1,
        int duration = 5,
        string notes = "")
    {
        return new CreateShipRequest
        {
            Name = name,
            Size = size,
            ArrivalDay = arrivalDay,
            Duration = duration,
            Notes = notes
        };
    }

    // ---------------------------------------------------------------
    // 1. Create_ShouldAddShipWithPendingStatus
    // ---------------------------------------------------------------
    [Fact]
    public void Create_ShouldAddShipWithPendingStatus()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var result = service.Create(MakeRequest());

        Assert.Equal(ShipStatus.Pending, result.Status);
    }

    // ---------------------------------------------------------------
    // 2. Create_ShouldPersistToDatabase
    // ---------------------------------------------------------------
    [Fact]
    public void Create_ShouldPersistToDatabase()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var created = service.Create(MakeRequest(name: "Persistent", size: ShipSize.L));

        // Verify via a fresh context read
        var persisted = context.Ships.Find(created.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Persistent", persisted.Name);
        Assert.Equal(ShipSize.L, persisted.Size);
        Assert.Equal(ShipStatus.Pending, persisted.Status);
    }

    // ---------------------------------------------------------------
    // 3. GetAll_ShouldReturnAllShips
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldReturnAllShips()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        service.Create(MakeRequest(name: "Alpha", arrivalDay: 1));
        service.Create(MakeRequest(name: "Beta", arrivalDay: 2));
        service.Create(MakeRequest(name: "Gamma", arrivalDay: 3));

        var all = service.GetAll();

        Assert.Equal(3, all.Count);
    }

    // ---------------------------------------------------------------
    // 4. GetAll_ShouldOrderByArrivalDay
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldOrderByArrivalDay()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        service.Create(MakeRequest(name: "Late", arrivalDay: 10));
        service.Create(MakeRequest(name: "Early", arrivalDay: 1));
        service.Create(MakeRequest(name: "Middle", arrivalDay: 5));

        var all = service.GetAll();

        Assert.Equal(1, all[0].ArrivalDay);
        Assert.Equal(5, all[1].ArrivalDay);
        Assert.Equal(10, all[2].ArrivalDay);
    }

    // ---------------------------------------------------------------
    // 5. GetById_ExistingId_ShouldReturnShip
    // ---------------------------------------------------------------
    [Fact]
    public void GetById_ExistingId_ShouldReturnShip()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var created = service.Create(MakeRequest(name: "FindMe"));

        var found = service.GetById(created.Id);

        Assert.NotNull(found);
        Assert.Equal("FindMe", found.Name);
    }

    // ---------------------------------------------------------------
    // 6. GetById_NonExistingId_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void GetById_NonExistingId_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var found = service.GetById(999);

        Assert.Null(found);
    }

    // ---------------------------------------------------------------
    // 7. GetById_ShouldIncludeAssignment
    // ---------------------------------------------------------------
    [Fact]
    public void GetById_ShouldIncludeAssignment()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var ship = service.Create(MakeRequest());

        var dock = new Dock { Name = "Dock A", Size = ShipSize.L };
        context.Docks.Add(dock);
        context.SaveChanges();

        var assignment = new Assignment
        {
            ShipId = ship.Id,
            DockId = dock.Id,
            StartDay = 1,
            EndDay = 5
        };
        context.Assignments.Add(assignment);
        context.SaveChanges();

        var found = service.GetById(ship.Id);

        Assert.NotNull(found);
        Assert.NotNull(found.Assignment);
        Assert.Equal(dock.Id, found.Assignment.DockId);
        Assert.NotNull(found.Assignment.Dock);
        Assert.Equal("Dock A", found.Assignment.Dock.Name);
    }

    // ---------------------------------------------------------------
    // 8. Delete_ExistingShip_ShouldReturnTrueAndRemove
    // ---------------------------------------------------------------
    [Fact]
    public void Delete_ExistingShip_ShouldReturnTrueAndRemove()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var created = service.Create(MakeRequest());

        var result = service.Delete(created.Id);

        Assert.True(result);
        Assert.Null(context.Ships.Find(created.Id));
    }

    // ---------------------------------------------------------------
    // 9. Delete_NonExistingShip_ShouldReturnFalse
    // ---------------------------------------------------------------
    [Fact]
    public void Delete_NonExistingShip_ShouldReturnFalse()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var result = service.Delete(999);

        Assert.False(result);
    }

    // ---------------------------------------------------------------
    // 10. Update_ExistingShip_ShouldApplyChanges
    // ---------------------------------------------------------------
    [Fact]
    public void Update_ExistingShip_ShouldApplyChanges()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var created = service.Create(MakeRequest(name: "OldName", duration: 3));

        var updated = service.Update(created.Id, ship =>
        {
            ship.Name = "NewName";
            ship.Duration = 10;
        });

        Assert.NotNull(updated);
        Assert.Equal("NewName", updated.Name);
        Assert.Equal(10, updated.Duration);

        // Verify persistence
        var fromDb = context.Ships.Find(created.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("NewName", fromDb.Name);
        Assert.Equal(10, fromDb.Duration);
    }

    // ---------------------------------------------------------------
    // 11. Update_NonExistingShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void Update_NonExistingShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var result = service.Update(999, ship =>
        {
            ship.Name = "Ghost";
        });

        Assert.Null(result);
    }
}
