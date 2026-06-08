using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.Models;

public sealed class Ship
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public ShipSize Size { get; set; } = ShipSize.Medium;

    [Range(1, 365)]
    public int ArrivalDay { get; set; } = 1;

    [Range(1, 30)]
    public int OccupationDurationDays { get; set; } = 1;

    public ShipStatus Status { get; set; } = ShipStatus.Pending;

    public ICollection<Assignment> Assignments { get; set; } = [];
}
