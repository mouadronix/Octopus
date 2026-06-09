using Octopus.Api.Models;

namespace Octopus.Api.Services;

public interface IShipService
{
    Task<IReadOnlyList<Ship>> GetAllAsync(CancellationToken ct = default);
    Task<Ship?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Ship> CreateAsync(Ship ship, CancellationToken ct = default);
    Task<Ship?> UpdateAsync(int id, Action<Ship> apply, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
