using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.Models;

public sealed class Assignment
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ShipId { get; set; }

    public Ship? Ship { get; set; }

    [Range(1, int.MaxValue)]
    public int BerthId { get; set; }

    public Berth? Berth { get; set; }

    [Range(1, 365)]
    public int StartDay { get; set; }

    [Range(1, 395)]
    public int EndDay { get; set; }
}
