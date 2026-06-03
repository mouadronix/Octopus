using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class BerthService(AppDbContext db)
{
    public IReadOnlyList<Berth> GetAll() => db.Berths;

    public Berth? GetById(int id) => db.Berths.FirstOrDefault(berth => berth.Id == id);
}
