namespace Octopus.Api.Models;

public class TerminalState
{
    public int Id { get; set; }
    public int CurrentDay { get; set; } = 1;
    public int PlanningHorizon { get; set; } = 30;
}
