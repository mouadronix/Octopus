namespace Octopus.Api.Models;

public sealed class Berth
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal MaxDraftMeters { get; set; }
    public bool IsAvailable { get; set; } = true;
}
