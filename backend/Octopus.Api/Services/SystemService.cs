using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public sealed class SystemService(AppDbContext db)
{
    public SystemState GetState()
    {
        var state = db.SystemStates.FirstOrDefault();
        if (state is not null)
        {
            return state;
        }

        state = new SystemState { CurrentDay = 1 };
        db.SystemStates.Add(state);
        db.SaveChanges();
        return state;
    }
}
