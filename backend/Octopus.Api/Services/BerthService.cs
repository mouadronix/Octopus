using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class BerthService(AppDbContext db)
{
    public IReadOnlyList<Berth> GetAll() =>
        db.Berths
            .Include(berth => berth.Assignments)
            .OrderBy(berth => berth.Size)
            .ThenBy(berth => berth.Name)
            .AsNoTracking()
            .ToList();

    public Berth? GetById(int id) =>
        db.Berths
            .Include(berth => berth.Assignments)
            .AsNoTracking()
            .FirstOrDefault(berth => berth.Id == id);
}
