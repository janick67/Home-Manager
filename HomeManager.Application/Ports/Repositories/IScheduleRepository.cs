using HomeManager.Domain.Models;

namespace HomeManager.Application.Ports.Repositories;

public interface IScheduleRepository
{
    Task<IReadOnlyCollection<ScheduleDefinition>> GetAllAsync(CancellationToken cancellationToken);
    Task<ScheduleDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ScheduleDefinition schedule, CancellationToken cancellationToken);
    Task UpdateAsync(ScheduleDefinition schedule, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
