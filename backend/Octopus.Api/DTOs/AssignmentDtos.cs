using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.DTOs;

public sealed class AssignShipRequest
{
    [Required, Range(1, int.MaxValue)]
    public int ShipId { get; init; }
}
