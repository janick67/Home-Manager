using HomeManager.Application.Contracts;

namespace HomeManager.Application.Ports.Repositories;

public interface ILogRepository
{
    Task AddAsync(LogEntryDto entry, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LogEntryDto>> GetRecentAsync(int count, CancellationToken cancellationToken);
}
