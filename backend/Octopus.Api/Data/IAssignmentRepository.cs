using Octopus.Api.Models;

namespace Octopus.Api.Data;

/// <summary>
/// Data-access seam for assignment operations.
/// Hides multi-entity fetching behind use-case-specific methods.
/// The repository adds entities; the caller controls save and transactions.
/// </summary>
public interface IAssignmentRepository
{
    /// <summary>
    /// Everything AssignShip needs: ship (with current assignment), dock, and terminal state.
    /// Returns null if any entity is missing.
    /// </summary>
    AssignmentContext? GetAssignmentContext(int shipId, int dockId);

    /// <summary>
    /// Everything GetSuggestion needs: ship, terminal state, all docks, and assignments grouped by dock.
    /// Returns null if ship or terminal is missing.
    /// </summary>
    SuggestionContext? GetSuggestionContext(int shipId);

    /// <summary>
    /// All assignments with ship and dock details, ordered by start day.
    /// </summary>
    List<Assignment> GetAllWithDetails();

    /// <summary>
    /// Add an assignment. Does NOT call SaveChanges — the caller controls when to save.
    /// </summary>
    void Add(Assignment assignment);

    /// <summary>
    /// Persist all pending changes.
    /// </summary>
    void Save();
}

/// <summary>
/// Data needed to assign a ship to a dock.
/// </summary>
public record AssignmentContext(Ship Ship, Dock Dock, TerminalState Terminal, List<Assignment> DockAssignments);

/// <summary>
/// Data needed to generate a scheduling suggestion.
/// </summary>
public record SuggestionContext(Ship Ship, TerminalState Terminal, List<Dock> AllDocks, Dictionary<int, List<Assignment>> AssignmentsByDock);
