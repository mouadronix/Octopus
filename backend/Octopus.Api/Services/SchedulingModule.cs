using Octopus.Api.DTOs;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

/// <summary>
/// Pure-function scheduling module. All scheduling rules live here.
/// No DB dependency — the caller pre-fetches data and passes it in.
/// </summary>
public static class SchedulingModule
{
    private static readonly Dictionary<ShipSize, int> SizeRank = new()
    {
        [ShipSize.S] = 1,
        [ShipSize.M] = 2,
        [ShipSize.L] = 3,
        [ShipSize.XL] = 4
    };

    /// <summary>
    /// First-fit greedy: find the earliest day a ship can be assigned to a dock.
    /// Scans existing assignments chronologically, finds the first gap that fits.
    /// </summary>
    public static (bool CanAssign, int StartDay) FindEarliestSlot(
        Ship ship, Dock dock, List<Assignment> dockAssignments, int currentDay, int horizon)
    {
        if (ship.ArrivalDay < currentDay)
            return (false, -1);
        if (ship.Duration < 1)
            return (false, -1);

        var sorted = dockAssignments.OrderBy(a => a.StartDay).ToList();
        int candidate = Math.Max(ship.ArrivalDay, currentDay);
        int maxDay = currentDay + horizon;

        foreach (var a in sorted)
        {
            int candidateEnd = candidate + ship.Duration - 1;
            if (candidateEnd < a.StartDay)
                return (true, candidate);
            candidate = a.EndDay + 1;
        }

        int finalEnd = candidate + ship.Duration - 1;
        return finalEnd <= maxDay ? (true, candidate) : (false, -1);
    }

    /// <summary>
    /// Filter docks to those whose size can accommodate the ship.
    /// </summary>
    public static List<Dock> CompatibleDocks(Ship ship, List<Dock> allDocks)
    {
        return allDocks.Where(d => SizeRank[d.Size] >= SizeRank[ship.Size]).ToList();
    }

    /// <summary>
    /// Find the best dock + start day for a ship across all compatible docks.
    /// Returns null if no dock can accommodate the ship.
    /// Also returns all compatible berths with their availability status.
    /// </summary>
    public static SuggestionResponse? Suggest(
        Ship ship, List<Dock> allDocks,
        Dictionary<int, List<Assignment>> assignmentsByDock,
        int currentDay, int horizon)
    {
        var compatible = CompatibleDocks(ship, allDocks);
        var candidates = new List<(Dock Dock, int StartDay, int AssignmentCount)>();
        var compatibleBerths = new List<CompatibleBerthDto>();

        foreach (var dock in compatible)
        {
            var dockAssignments = assignmentsByDock.GetValueOrDefault(dock.Id, new List<Assignment>());
            var (canAssign, startDay) = FindEarliestSlot(ship, dock, dockAssignments, currentDay, horizon);

            // Track all compatible berths for the frontend
            compatibleBerths.Add(new CompatibleBerthDto
            {
                DockId = dock.Id,
                DockName = dock.Name,
                Size = dock.Size.ToString(),
                StartDay = canAssign ? startDay : -1,
                Available = canAssign
            });

            if (canAssign)
                candidates.Add((dock, startDay, dockAssignments.Count));
        }

        if (candidates.Count == 0)
            return null;

        var best = candidates
            .OrderBy(c => c.StartDay)
            .ThenBy(c => c.AssignmentCount)
            .First();

        return new SuggestionResponse
        {
            DockId = best.Dock.Id,
            DockName = best.Dock.Name,
            StartDay = best.StartDay,
            Message = best.StartDay <= ship.ArrivalDay
                ? $"Available from Day {best.StartDay}"
                : $"Delayed: earliest slot Day {best.StartDay}",
            CompatibleBerths = compatibleBerths
                .Where(b => b.Available)
                .OrderBy(b => b.StartDay)
                .ToList()
        };
    }
}
