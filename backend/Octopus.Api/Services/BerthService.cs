using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class BerthService(AppDbContext db)
{
    public IReadOnlyList<Berth> GetAll() => db.Berths;

    public Berth? GetById(int id) => db.Berths.FirstOrDefault(berth => berth.Id == id);

    public Berth Create(Berth berth)
    {
        berth.Id = db.Berths.Count == 0 ? 1 : db.Berths.Max(existing => existing.Id) + 1;
        db.Berths.Add(berth);
        return berth;
    }

    public Berth? Update(int id, Action<Berth> apply)
    {
        var berth = GetById(id);
        if (berth is null) return null;
        apply(berth);
        return berth;
    }

    public bool Delete(int id)
    {
        var berth = GetById(id);
        if (berth is null) return false;
        db.Berths.Remove(berth);
        return true;
    }
}
