namespace Octopus.Api.Models;

public sealed class SystemState
{
    public string Environment { get; set; } = "Development";
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public int CurrentDay { get; set; }
    public int ShipCount { get; set; }
    public int BerthCount { get; set; }
    public int ActiveAssignmentCount { get; set; }
}
