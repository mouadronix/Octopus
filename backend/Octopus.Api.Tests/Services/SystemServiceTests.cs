using Xunit;
using Octopus.Api.Models;
using Octopus.Api.Services;
using Octopus.Api.Tests.Helpers;

namespace Octopus.Api.Tests.Services;

public class SystemServiceTests
{
    // ---------------------------------------------------------------
    // 1. GetCurrentDay_ShouldReturnCurrentDay
    // ---------------------------------------------------------------
    [Fact]
    public void GetCurrentDay_ShouldReturnCurrentDay()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 7, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new SystemService(context);

        var result = service.GetCurrentDay();

        Assert.Equal(7, result);
    }

    // ---------------------------------------------------------------
    // 2. GetState_ShouldReturnCorrectCounts
    // ---------------------------------------------------------------
    [Fact]
    public void GetState_ShouldReturnCorrectCounts()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 3, PlanningHorizon = 30 });
        context.Ships.Add(new Ship { Name = "A", Status = ShipStatus.Pending, Size = ShipSize.M, ArrivalDay = 1, Duration = 5 });
        context.Ships.Add(new Ship { Name = "B", Status = ShipStatus.Assigned, Size = ShipSize.L, ArrivalDay = 2, Duration = 3 });
        context.Docks.Add(new Dock { Name = "D1", Size = ShipSize.L });
        context.Docks.Add(new Dock { Name = "D2", Size = ShipSize.M });
        context.Docks.Add(new Dock { Name = "D3", Size = ShipSize.S });
        context.Assignments.Add(new Assignment { ShipId = 2, DockId = 1, StartDay = 1, EndDay = 5 });
        context.SaveChanges();

        var service = new SystemService(context);

        var state = service.GetState();

        Assert.Equal(3, state.CurrentDay);
        Assert.Equal(2, state.ShipCount);
        Assert.Equal(3, state.BerthCount);
        Assert.Equal(1, state.ActiveAssignmentCount);
    }

    // ---------------------------------------------------------------
    // 3. AdvanceDay_ShouldIncrementCurrentDay
    // ---------------------------------------------------------------
    [Fact]
    public void AdvanceDay_ShouldIncrementCurrentDay()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 5, PlanningHorizon = 30 });
        context.SaveChanges();

        var service = new SystemService(context);

        var result = service.AdvanceDay();

        Assert.Equal(6, result.CurrentDay);
    }

    // ---------------------------------------------------------------
    // 4. AdvanceDay_NoTerminalState_ShouldCreateOne
    // ---------------------------------------------------------------
    [Fact]
    public void AdvanceDay_NoTerminalState_ShouldCreateOne()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        // No TerminalState seeded — the service should create one

        var service = new SystemService(context);

        var result = service.AdvanceDay();

        // Default CurrentDay = 1, then incremented to 2
        Assert.Equal(2, result.CurrentDay);
        Assert.Single(context.TerminalStates);
    }

    // ---------------------------------------------------------------
    // 5. AdvanceDay_ShipPastEndDay_ShouldMarkDeparted
    // ---------------------------------------------------------------
    [Fact]
    public void AdvanceDay_ShipPastEndDay_ShouldMarkDeparted()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 4, PlanningHorizon = 30 });
        var ship = new Ship { Name = "Leaving", Status = ShipStatus.Assigned, Size = ShipSize.M, ArrivalDay = 1, Duration = 3 };
        context.Ships.Add(ship);
        context.SaveChanges();

        context.Assignments.Add(new Assignment { ShipId = ship.Id, DockId = 1, StartDay = 1, EndDay = 4 });
        context.SaveChanges();

        var service = new SystemService(context);

        service.AdvanceDay(); // CurrentDay becomes 5, EndDay (4) < 5

        var updated = context.Ships.Find(ship.Id)!;
        Assert.Equal(ShipStatus.Departed, updated.Status);
    }

    // ---------------------------------------------------------------
    // 6. AdvanceDay_ShipNotPastEndDay_ShouldStayAssigned
    // ---------------------------------------------------------------
    [Fact]
    public void AdvanceDay_ShipNotPastEndDay_ShouldStayAssigned()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 4, PlanningHorizon = 30 });
        var ship = new Ship { Name = "Staying", Status = ShipStatus.Assigned, Size = ShipSize.M, ArrivalDay = 1, Duration = 10 };
        context.Ships.Add(ship);
        context.SaveChanges();

        context.Assignments.Add(new Assignment { ShipId = ship.Id, DockId = 1, StartDay = 1, EndDay = 10 });
        context.SaveChanges();

        var service = new SystemService(context);

        service.AdvanceDay(); // CurrentDay becomes 5, EndDay (10) >= 5

        var updated = context.Ships.Find(ship.Id)!;
        Assert.Equal(ShipStatus.Assigned, updated.Status);
    }

    // ---------------------------------------------------------------
    // 7. AdvanceDay_MultipleAdvances_ShouldTrackCorrectly
    // ---------------------------------------------------------------
    [Fact]
    public void AdvanceDay_MultipleAdvances_ShouldTrackCorrectly()
    {
        using var context = TestDbContextFactory.CreateDbContext();
        context.TerminalStates.Add(new TerminalState { CurrentDay = 1, PlanningHorizon = 30 });
        var earlyShip = new Ship { Name = "Early", Status = ShipStatus.Assigned, Size = ShipSize.S, ArrivalDay = 1, Duration = 2 };
        var lateShip = new Ship { Name = "Late", Status = ShipStatus.Assigned, Size = ShipSize.L, ArrivalDay = 1, Duration = 10 };
        context.Ships.AddRange(earlyShip, lateShip);
        context.SaveChanges();

        context.Assignments.Add(new Assignment { ShipId = earlyShip.Id, DockId = 1, StartDay = 1, EndDay = 2 });
        context.Assignments.Add(new Assignment { ShipId = lateShip.Id, DockId = 1, StartDay = 1, EndDay = 10 });
        context.SaveChanges();

        var service = new SystemService(context);

        // Advance 1: CurrentDay -> 2
        var state1 = service.AdvanceDay();
        Assert.Equal(2, state1.CurrentDay);
        Assert.Equal(ShipStatus.Assigned, context.Ships.Find(earlyShip.Id)!.Status);
        Assert.Equal(ShipStatus.Assigned, context.Ships.Find(lateShip.Id)!.Status);

        // Advance 2: CurrentDay -> 3, earlyShip.EndDay (2) < 3 => Departed
        var state2 = service.AdvanceDay();
        Assert.Equal(3, state2.CurrentDay);
        Assert.Equal(ShipStatus.Departed, context.Ships.Find(earlyShip.Id)!.Status);
        Assert.Equal(ShipStatus.Assigned, context.Ships.Find(lateShip.Id)!.Status);

        // Advance 3: CurrentDay -> 4
        var state3 = service.AdvanceDay();
        Assert.Equal(4, state3.CurrentDay);
        Assert.Equal(ShipStatus.Departed, context.Ships.Find(earlyShip.Id)!.Status);
        Assert.Equal(ShipStatus.Assigned, context.Ships.Find(lateShip.Id)!.Status);
    }
}
