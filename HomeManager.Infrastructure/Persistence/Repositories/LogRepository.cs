using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class LogRepository : ILogRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public LogRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LogEntryDto entry, CancellationToken cancellationToken)
    {
        _dbContext.Logs.Add(new LogEntryEntity
        {
            Id = Guid.NewGuid(),
            TimestampUtc = entry.TimestampUtc,
            Level = entry.Level,
            Message = entry.Message,
            Details = entry.Details
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LogEntryDto>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        return await _dbContext.Logs
            .OrderByDescending(x => x.TimestampUtc)
            .Take(count)
            .Select(x => new LogEntryDto(
                x.TimestampUtc,
                x.Level,
                x.Message,
                x.Details))
            .ToArrayAsync(cancellationToken);
    }
}
