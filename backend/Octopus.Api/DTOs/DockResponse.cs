using Octopus.Api.Models;

namespace Octopus.Api.DTOs;

public class DockResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ShipSize Size { get; set; }
    public List<AssignmentInfo> Assignments { get; set; } = new();
}

public class AssignmentInfo
{
    public int ShipId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public int EndDay { get; set; }
}
