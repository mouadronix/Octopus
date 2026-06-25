using System.ComponentModel.DataAnnotations;
using Octopus.Api.Models;

namespace Octopus.Api.DTOs;

public class CreateShipRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    [Required]
    public ShipSize Size { get; set; }

    [Range(1, 365)]
    public int ArrivalDay { get; set; } = 1;

    [Range(1, 90)]
    public int Duration { get; set; } = 1;

    [Url]
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }
}
