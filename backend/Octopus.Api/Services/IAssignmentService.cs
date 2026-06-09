using Octopus.Api.Models;

namespace Octopus.Api.Services;

public interface IAssignmentService
{
    Task<IReadOnlyList<Assignment>> GetAllAsync(CancellationToken ct = default);
    Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Assignment> CreateAsync(Assignment assignment, CancellationToken ct = default);
    Task<Assignment?> UpdateAsync(int id, Action<Assignment> apply, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
