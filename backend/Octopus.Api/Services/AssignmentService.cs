using Octopus.Api.Data;
using Octopus.Api.DTOs;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class AssignmentService
{
    private readonly IAssignmentRepository _repo;
    private readonly AppDbContext _context;

    public AssignmentService(IAssignmentRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public List<Assignment> GetAll()
    {
        return _repo.GetAllWithDetails();
    }

    /// <summary>
    /// Assign a ship to a specific dock.
    /// Spec: uses database transaction, checks planning horizon.
    /// </summary>
    public Assignment? AssignShip(int shipId, int dockId)
    {
        var ctx = _repo.GetAssignmentContext(shipId, dockId);
        if (ctx is null)
            return null;

        if (ctx.Ship.Status != ShipStatus.Pending || ctx.Ship.Assignment is not null)
            return null;

        if (!CanFitShip(ctx.Dock.Size, ctx.Ship.Size))
            return null;

        var (canAssign, startDay) = SchedulingModule.FindEarliestSlot(
            ctx.Ship, ctx.Dock, ctx.DockAssignments,
            ctx.Terminal.CurrentDay, ctx.Terminal.PlanningHorizon);

        if (!canAssign)
            return null;

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var assignment = new Assignment
            {
                ShipId = ctx.Ship.Id,
                DockId = ctx.Dock.Id,
                StartDay = startDay,
                EndDay = startDay + ctx.Ship.Duration - 1
            };

            ctx.Ship.Status = ShipStatus.Assigned;
            _repo.Add(assignment);
            _repo.Save();
            transaction.Commit();

            return assignment;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Get a scheduling suggestion for a ship. Delegates scheduling to SchedulingModule.
    /// </summary>
    public SuggestionResponse? GetSuggestion(int shipId)
    {
        var ctx = _repo.GetSuggestionContext(shipId);
        if (ctx is null || ctx.Ship.Status != ShipStatus.Pending)
            return null;

        return SchedulingModule.Suggest(
            ctx.Ship, ctx.AllDocks, ctx.AssignmentsByDock,
            ctx.Terminal.CurrentDay, ctx.Terminal.PlanningHorizon);
    }

    private static bool CanFitShip(ShipSize dockSize, ShipSize shipSize)
    {
        return SizeRank(dockSize) >= SizeRank(shipSize);
    }

    private static int SizeRank(ShipSize size)
    {
        return size switch
        {
            ShipSize.S => 1,
            ShipSize.M => 2,
            ShipSize.L => 3,
            ShipSize.XL => 4,
            _ => 0
        };
    }
}
