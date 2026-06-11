namespace Octopus.Api.DTOs;

public class SuggestionResponse
{
    public int DockId { get; set; }
    public string DockName { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public string Message { get; set; } = string.Empty;
}
