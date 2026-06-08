using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.DTOs;

public sealed class CreateShipRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, RegularExpression(@"^IMO-\d{7}$", ErrorMessage = "ImoNumber must match format IMO-XXXXXXX")]
    public required string ImoNumber { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string CargoType { get; init; }

    [Required]
    public DateTime EstimatedArrival { get; init; }
}

public sealed class UpdateShipRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Name { get; init; }

    [RegularExpression(@"^IMO-\d{7}$", ErrorMessage = "ImoNumber must match format IMO-XXXXXXX")]
    public string? ImoNumber { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? CargoType { get; init; }

    public DateTime? EstimatedArrival { get; init; }

    [StringLength(50)]
    public string? Status { get; init; }
}
