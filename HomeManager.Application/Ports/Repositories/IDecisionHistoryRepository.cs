using HomeManager.Domain.Models;

namespace HomeManager.Application.Ports.Repositories;

public interface IDecisionHistoryRepository
{
    Task AddAsync(PowerManagerDecisionResult decisionResult, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PowerManagerDecisionResult>> GetLatestAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PowerManagerDecisionResult>> GetHistoryAsync(DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
}
