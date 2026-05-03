using HomeManager.Application.Ports.Repositories;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class ManagerStateRepository : IManagerStateRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public ManagerStateRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DateTimeOffset?> GetLastHeartbeatAsync(string managerName, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ManagerStates.FirstOrDefaultAsync(x => x.ManagerName == managerName, cancellationToken);
        return entity?.LastHeartbeatAtUtc;
    }

    public async Task SetLastHeartbeatAsync(string managerName, DateTimeOffset timestampUtc, CancellationToken cancellationToken)
    {
        var entity = await EnsureStateAsync(managerName, cancellationToken);
        entity.LastHeartbeatAtUtc = timestampUtc;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DateTimeOffset?> GetLastEvaluationAsync(string managerName, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ManagerStates.FirstOrDefaultAsync(x => x.ManagerName == managerName, cancellationToken);
        return entity?.LastEvaluationAtUtc;
    }

    public async Task SetLastEvaluationAsync(string managerName, DateTimeOffset timestampUtc, CancellationToken cancellationToken)
    {
        var entity = await EnsureStateAsync(managerName, cancellationToken);
        entity.LastEvaluationAtUtc = timestampUtc;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ManagerStateEntity> EnsureStateAsync(string managerName, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ManagerStates.FirstOrDefaultAsync(x => x.ManagerName == managerName, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var entity = new ManagerStateEntity { ManagerName = managerName };
        _dbContext.ManagerStates.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
