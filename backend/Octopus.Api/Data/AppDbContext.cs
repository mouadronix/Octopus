using Octopus.Api.Models;

namespace Octopus.Api.Data;

public sealed class AppDbContext
{
    public List<Ship> Ships { get; } = [];
    public List<Berth> Berths { get; } = [];
    public List<Assignment> Assignments { get; } = [];
}
