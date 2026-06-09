using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class SystemService(AppDbContext db, IWebHostEnvironment environment) : ISystemService
{
    public SystemState GetState() => new()
    {
        Environment = environment.EnvironmentName,
        ServerTimeUtc = DateTime.UtcNow,
        ShipCount = db.Ships.Count(),
        BerthCount = db.Berths.Count(),
        ActiveAssignmentCount = db.Assignments.Count(a => a.EndsAt == null)
    };
}
