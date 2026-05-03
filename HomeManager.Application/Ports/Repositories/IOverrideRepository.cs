using HomeManager.Domain.Models;

namespace HomeManager.Application.Ports.Repositories;

public interface IOverrideRepository
{
    Task<IReadOnlyCollection<ManualOverrideDefinition>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(ManualOverrideDefinition entry, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
