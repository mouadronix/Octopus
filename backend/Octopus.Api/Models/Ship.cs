namespace Octopus.Api.Models;

public class Ship
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ShipSize Size { get; set; }
    public ShipStatus Status { get; set; } = ShipStatus.Pending;
    public int ArrivalDay { get; set; }
    public int Duration { get; set; }
    public string? ImageUrl { get; set; }

    // Navigation property
    public Assignment? Assignment { get; set; }
}
