using System.ComponentModel.DataAnnotations;

namespace Octopus.Api.Models;

public sealed class Berth
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public required string Name { get; set; }

    public BerthSize Size { get; set; } = BerthSize.Medium;

    public ICollection<Assignment> Assignments { get; set; } = [];
}
