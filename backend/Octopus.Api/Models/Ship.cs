using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.Models;

public class Ship
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ShipSize Size { get; set; }
    public ShipStatus Status { get; set; } = ShipStatus.Pending;
    public int ArrivalDay { get; set; }
    [Range(3, 15)]
    public int Duration { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation property
    public Assignment? Assignment { get; set; }
}
