using Octopus.Api.Models;

namespace Octopus.Api.Services;

public interface IBerthService
{
    Task<IReadOnlyList<Berth>> GetAllAsync(CancellationToken ct = default);
    Task<Berth?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Berth> CreateAsync(Berth berth, CancellationToken ct = default);
    Task<Berth?> UpdateAsync(int id, Action<Berth> apply, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
