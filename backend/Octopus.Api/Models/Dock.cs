namespace Octopus.Api.Models;

public class Dock
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ShipSize Size { get; set; }

    // Navigation property
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
