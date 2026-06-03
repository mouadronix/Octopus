namespace Octopus.Api.Models;

public sealed class Ship
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ImoNumber { get; set; }
    public required string CargoType { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public string Status { get; set; } = "Waiting";
}
