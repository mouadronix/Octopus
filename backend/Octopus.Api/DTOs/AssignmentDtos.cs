using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.DTOs;

public sealed class CreateAssignmentRequest
{
    [Required, Range(1, int.MaxValue)]
    public int ShipId { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int DockId { get; init; }

    [Required]
    public DateTime StartsAt { get; init; }

    [StringLength(50)]
    public string Status { get; init; } = "Planned";
}

public sealed class AssignShipRequest
{
    [Required, Range(1, int.MaxValue)]
    public int ShipId { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int DockId { get; init; }
}

public sealed class UpdateAssignmentRequest
{
    [Range(1, int.MaxValue)]
    public int? ShipId { get; init; }

    [Range(1, int.MaxValue)]
    public int? DockId { get; init; }

    public DateTime? StartsAt { get; init; }

    public DateTime? EndsAt { get; init; }

    [StringLength(50)]
    public string? Status { get; init; }
}
