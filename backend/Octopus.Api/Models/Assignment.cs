namespace Octopus.Api.Models;

public sealed class Assignment
{
    public int Id { get; set; }
    public int ShipId { get; set; }
    public int BerthId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Status { get; set; } = "Planned";
}
