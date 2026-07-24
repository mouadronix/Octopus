using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.DTOs;

public sealed class CreateBerthRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, Range(0.1, 999.9)]
    public decimal MaxDraftMeters { get; init; }
}

public sealed class UpdateBerthRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Name { get; init; }

    [Range(0.1, 999.9)]
    public decimal? MaxDraftMeters { get; init; }

    public bool? IsAvailable { get; init; }
}
