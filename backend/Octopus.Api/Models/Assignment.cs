namespace Octopus.Api.Models;

public class Assignment
{
    public int Id { get; set; }
    public int ShipId { get; set; }
    public int DockId { get; set; }
    public int StartDay { get; set; }
    public int EndDay { get; set; }

    // Navigation properties
    public Ship Ship { get; set; } = null!;
    public Dock Dock { get; set; } = null!;
}
