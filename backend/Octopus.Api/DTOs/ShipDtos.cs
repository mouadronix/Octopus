using Octopus.Api.Models;

namespace Octopus.Api.DTOs;

public class ShipListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ShipSize Size { get; set; }
    public ShipStatus Status { get; set; }
    public int ArrivalDay { get; set; }
    public int Duration { get; set; }
    public string? BerthName { get; set; }
    public int? AssignmentId { get; set; }
    public int? AssignmentStartDay { get; set; }
    public int? AssignmentEndDay { get; set; }
    public string? ImageUrl { get; set; }
}
