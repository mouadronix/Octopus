using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.DTOs;

public class EditShipRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}
