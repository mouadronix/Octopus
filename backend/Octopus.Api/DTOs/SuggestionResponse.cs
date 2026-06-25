namespace Octopus.Api.DTOs;

public class SuggestionResponse
{
    public int DockId { get; set; }
    public string DockName { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// All compatible berths with their availability status.
    /// </summary>
    public List<CompatibleBerthDto> CompatibleBerths { get; set; } = new();
}

public class CompatibleBerthDto
{
    public int DockId { get; set; }
    public string DockName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public bool Available { get; set; }
}
