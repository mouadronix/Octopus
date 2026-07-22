using Xunit;
using Octopus.Api.Data;
using Octopus.Api.Models;
using Octopus.Api.Services;
using Octopus.Api.Tests.Helpers;

namespace Octopus.Api.Tests.Services;

public class AssignmentServiceTests
{
    private static void SeedTerminal(AppDbContext context, int currentDay = 1)
    {
        context.TerminalStates.Add(new TerminalState { Id = 1, CurrentDay = currentDay, PlanningHorizon = 60 });
        context.SaveChanges();
    }

    private static Ship CreateShip(
        string name = "TestShip",
        ShipSize size = ShipSize.M,
        ShipStatus status = ShipStatus.Pending,
        int arrivalDay = 1,
        int duration = 5)
    {
        return new Ship
        {
            Name = name,
            Size = size,
            Status = status,
            ArrivalDay = arrivalDay,
            Duration = duration
        };
    }

    private static Dock CreateDock(string name = "Dock A", ShipSize size = ShipSize.L)
    {
        return new Dock { Name = name, Size = size };
    }

    // ---------------------------------------------------------------
    // 1. GetAll_ShouldReturnAllAssignments
    // ---------------------------------------------------------------
    [Fact]
    public void GetAll_ShouldReturnAllAssignments()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);

        var ship1 = CreateShip(name: "Ship1");
        var ship2 = CreateShip(name: "Ship2");
        var dock = CreateDock();
        context.Ships.AddRange(ship1, ship2);
        context.Docks.Add(dock);
        context.SaveChanges();

        context.Assignments.AddRange(
            new Assignment { ShipId = ship1.Id, DockId = dock.Id, StartDay = 1, EndDay = 5 },
            new Assignment { ShipId = ship2.Id, DockId = dock.Id, StartDay = 6, EndDay = 10 }
        );
        context.SaveChanges();

        var all = service.GetAll();

        Assert.Equal(2, all.Count);
    }

    // ---------------------------------------------------------------
    // 2. AssignShip_ValidInputs_ShouldCreateAssignment
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_ValidInputs_ShouldCreateAssignment()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(size: ShipSize.M, arrivalDay: 1, duration: 3);
        var dock = CreateDock(size: ShipSize.L);
        context.Ships.Add(ship);
        context.Docks.Add(dock);
        context.SaveChanges();

        var result = service.AssignShip(ship.Id, dock.Id);

        Assert.NotNull(result);
        Assert.Equal(ship.Id, result.ShipId);
        Assert.Equal(dock.Id, result.DockId);
    }

    // ---------------------------------------------------------------
    // 3. AssignShip_ShouldSetCorrectStartAndEndDay
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_ShouldSetCorrectStartAndEndDay()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context, currentDay: 3);

        var ship = CreateShip(arrivalDay: 1, duration: 4);
        var dock = CreateDock();
        context.Ships.Add(ship);
        context.Docks.Add(dock);
        context.SaveChanges();

        var result = service.AssignShip(ship.Id, dock.Id);

        Assert.NotNull(result);
        // StartDay = max(ArrivalDay=1, CurrentDay=3) = 3
        Assert.Equal(3, result.StartDay);
        // EndDay = StartDay + Duration - 1 = 3 + 4 - 1 = 6
        Assert.Equal(6, result.EndDay);
    }

    // ---------------------------------------------------------------
    // 4. AssignShip_ShouldSetShipStatusToAssigned
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_ShouldSetShipStatusToAssigned()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip();
        var dock = CreateDock();
        context.Ships.Add(ship);
        context.Docks.Add(dock);
        context.SaveChanges();

        service.AssignShip(ship.Id, dock.Id);

        var updatedShip = context.Ships.Find(ship.Id);
        Assert.NotNull(updatedShip);
        Assert.Equal(ShipStatus.Assigned, updatedShip.Status);
    }

    // ---------------------------------------------------------------
    // 5. AssignShip_NonExistentShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_NonExistentShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var dock = CreateDock();
        context.Docks.Add(dock);
        context.SaveChanges();

        var result = service.AssignShip(999, dock.Id);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 6. AssignShip_NonExistentDock_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_NonExistentDock_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip();
        context.Ships.Add(ship);
        context.SaveChanges();

        var result = service.AssignShip(ship.Id, 999);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 7. AssignShip_AlreadyAssignedShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_AlreadyAssignedShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(status: ShipStatus.Assigned);
        var dock = CreateDock();
        context.Ships.Add(ship);
        context.Docks.Add(dock);
        context.SaveChanges();

        context.Assignments.Add(new Assignment
        {
            ShipId = ship.Id,
            DockId = dock.Id,
            StartDay = 1,
            EndDay = 5
        });
        context.SaveChanges();

        var dock2 = CreateDock(name: "Dock B");
        context.Docks.Add(dock2);
        context.SaveChanges();

        var result = service.AssignShip(ship.Id, dock2.Id);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 8. AssignShip_ShipTooLargeForDock_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_ShipTooLargeForDock_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(size: ShipSize.XL);
        var dock = CreateDock(size: ShipSize.S);
        context.Ships.Add(ship);
        context.Docks.Add(dock);
        context.SaveChanges();

        var result = service.AssignShip(ship.Id, dock.Id);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 9. AssignShip_DockConflict_ShouldFindNextAvailableSlot
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_DockConflict_ShouldFindNextAvailableSlot()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var existingShip = CreateShip(name: "Existing", arrivalDay: 1, duration: 5);
        var newShip = CreateShip(name: "New", arrivalDay: 3, duration: 3);
        var dock = CreateDock();
        context.Ships.AddRange(existingShip, newShip);
        context.Docks.Add(dock);
        context.SaveChanges();

        // Block days 1-5 on this dock
        context.Assignments.Add(new Assignment
        {
            ShipId = existingShip.Id,
            DockId = dock.Id,
            StartDay = 1,
            EndDay = 5
        });
        context.SaveChanges();

        // New ship wants days 3-5 (overlaps with 1-5) — should be pushed to day 6
        var result = service.AssignShip(newShip.Id, dock.Id);

        Assert.NotNull(result);
        // earliestDay = max(ArrivalDay=3, CurrentDay=1) = 3
        // Dock blocked 1-5, so slot found at 6-8
        Assert.Equal(6, result.StartDay);
        Assert.Equal(8, result.EndDay);
    }

    // ---------------------------------------------------------------
    // 10. AssignShip_ShipArrivesAfterCurrentDay_ShouldUseArrivalDay
    // ---------------------------------------------------------------
    [Fact]
    public void AssignShip_ShipArrivesAfterCurrentDay_ShouldUseArrivalDay()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context, currentDay: 1);

        var ship = CreateShip(arrivalDay: 5, duration: 3);
        var dock = CreateDock();
        context.Ships.Add(ship);
        context.Docks.Add(dock);
        context.SaveChanges();

        var result = service.AssignShip(ship.Id, dock.Id);

        Assert.NotNull(result);
        // StartDay = max(ArrivalDay=5, CurrentDay=1) = 5
        Assert.Equal(5, result.StartDay);
        // EndDay = 5 + 3 - 1 = 7
        Assert.Equal(7, result.EndDay);
    }

    // ---------------------------------------------------------------
    // 11. GetSuggestion_ValidPendingShip_ShouldReturnFirstFittingDock
    // ---------------------------------------------------------------
    [Fact]
    public void GetSuggestion_ValidPendingShip_ShouldReturnFirstFittingDock()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(size: ShipSize.M, arrivalDay: 1, duration: 3);
        var smallDock = CreateDock(name: "Small", size: ShipSize.S);
        var mediumDock = CreateDock(name: "Medium", size: ShipSize.M);
        var largeDock = CreateDock(name: "Large", size: ShipSize.L);
        context.Ships.Add(ship);
        context.Docks.AddRange(smallDock, mediumDock, largeDock);
        context.SaveChanges();

        var result = service.GetSuggestion(ship.Id);

        Assert.NotNull(result);
        // First-fit: scan in DB order, pick first that fits
        // Small doesn't fit (S < M), Medium fits → pick Medium
        Assert.Equal(mediumDock.Id, result.DockId);
        Assert.Equal("Medium", result.DockName);
        Assert.Equal(1, result.StartDay);
    }

    // ---------------------------------------------------------------
    // 12. GetSuggestion_NonExistentShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void GetSuggestion_NonExistentShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var result = service.GetSuggestion(999);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 13. GetSuggestion_AlreadyAssignedShip_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void GetSuggestion_AlreadyAssignedShip_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(status: ShipStatus.Assigned);
        context.Ships.Add(ship);
        context.SaveChanges();

        var result = service.GetSuggestion(ship.Id);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 14. GetSuggestion_NoDockAvailable_ShouldReturnNull
    // ---------------------------------------------------------------
    [Fact]
    public void GetSuggestion_NoDockAvailable_ShouldReturnNull()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(size: ShipSize.XL, arrivalDay: 1, duration: 3);
        var smallDock = CreateDock(name: "Small", size: ShipSize.S);
        context.Ships.Add(ship);
        context.Docks.Add(smallDock);
        context.SaveChanges();

        var result = service.GetSuggestion(ship.Id);

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // 15. GetSuggestion_ShouldUseFirstFit
    // ---------------------------------------------------------------
    [Fact]
    public void GetSuggestion_ShouldUseFirstFit()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        var service = new AssignmentService(new EfAssignmentRepository(context), context);
        SeedTerminal(context);

        var ship = CreateShip(size: ShipSize.M, arrivalDay: 1, duration: 2);
        var largeDock = CreateDock(name: "Zulu", size: ShipSize.XL);
        var mediumDock = CreateDock(name: "Alpha", size: ShipSize.M);
        context.Ships.Add(ship);
        context.Docks.AddRange(largeDock, mediumDock);
        context.SaveChanges();

        var result = service.GetSuggestion(ship.Id);

        Assert.NotNull(result);
        // First-fit: Zulu (XL) is first in DB order and fits M → pick Zulu
        Assert.Equal(largeDock.Id, result.DockId);
        Assert.Equal("Zulu", result.DockName);
    }
}
