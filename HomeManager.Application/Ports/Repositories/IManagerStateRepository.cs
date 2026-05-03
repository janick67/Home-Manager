namespace HomeManager.Application.Ports.Repositories;

public interface IManagerStateRepository
{
    Task<DateTimeOffset?> GetLastHeartbeatAsync(string managerName, CancellationToken cancellationToken);
    Task SetLastHeartbeatAsync(string managerName, DateTimeOffset timestampUtc, CancellationToken cancellationToken);
    Task<DateTimeOffset?> GetLastEvaluationAsync(string managerName, CancellationToken cancellationToken);
    Task SetLastEvaluationAsync(string managerName, DateTimeOffset timestampUtc, CancellationToken cancellationToken);
}
