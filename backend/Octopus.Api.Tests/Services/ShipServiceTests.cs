using System;
using Xunit;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;
using Octopus.Api.Tests.Helpers;

namespace Octopus.Api.Tests.Services;

public class ShipServiceTests
{
    private static CreateShipRequest MakeRequest(string name = "TestShip", string notes = "")
    {
        return new CreateShipRequest
        {
            Name = name,
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
        // Ensure TerminalState exists for auto-generation
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

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
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new ShipService(context);
        var created = service.Create(MakeRequest(name: "Persistent"));

        var persisted = context.Ships.Find(created.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Persistent", persisted.Name);
        Assert.Equal(ShipStatus.Pending, persisted.Status);
    }

    // ---------------------------------------------------------------
    // 3. Create_ShouldAutoGenerateFields
    // ---------------------------------------------------------------
    [Fact]
    public void Create_ShouldAutoGenerateFields()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 10, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new ShipService(context);
        var created = service.Create(MakeRequest());

        Assert.True(Enum.IsDefined(created.Size));
        Assert.True(created.ArrivalDay >= 10); // currentDay + random(0..30)
        Assert.InRange(created.Duration, 3, 15);
    }

    // ---------------------------------------------------------------
    // 4. GetAll_ShouldReturnAllShips
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldReturnAllShips()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new ShipService(context);
        service.Create(MakeRequest(name: "Alpha"));
        service.Create(MakeRequest(name: "Beta"));
        service.Create(MakeRequest(name: "Gamma"));

        var all = service.GetAll();
        Assert.Equal(3, all.Count);
    }

    // ---------------------------------------------------------------
    // 5. GetById_ExistingId_ShouldReturnShip
    // ---------------------------------------------------------------
    [Fact]
    public void GetById_ExistingId_ShouldReturnShip()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

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
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

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
    // 8. Update_PendingShip_ShouldApplyChanges
    // ---------------------------------------------------------------
    [Fact]
    public void Update_PendingShip_ShouldApplyChanges()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new ShipService(context);
        var created = service.Create(MakeRequest(name: "OldName"));

        var updated = service.Update(created.Id, "NewName", "New notes");

        Assert.NotNull(updated);
        Assert.Equal("NewName", updated.Name);
        Assert.Equal("New notes", updated.Notes);

        var fromDb = context.Ships.Find(created.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("NewName", fromDb.Name);
        Assert.Equal("New notes", fromDb.Notes);
    }

    // ---------------------------------------------------------------
    // 9. Update_NonPendingShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void Update_NonPendingShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new ShipService(context);
        var ship = new Ship { Name = "Assigned", Size = ShipSize.M, Status = ShipStatus.Assigned, ArrivalDay = 1, Duration = 5 };
        context.Ships.Add(ship);
        context.SaveChanges();

        var result = service.Update(ship.Id, "New", "Notes");
        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 10. Update_NonExistingShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void Update_NonExistingShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new ShipService(context);

        var result = service.Update(999, "Ghost", "Notes");
        Assert.Null(result);
    }
}
