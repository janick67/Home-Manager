using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public ScheduleRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ScheduleDefinition>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Schedules
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomain).ToArray();
    }

    public async Task<ScheduleDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Schedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task AddAsync(ScheduleDefinition schedule, CancellationToken cancellationToken)
    {
        _dbContext.Schedules.Add(ToEntity(schedule));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ScheduleDefinition schedule, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Schedules.FirstOrDefaultAsync(x => x.Id == schedule.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Schedule {schedule.Id} not found.");

        entity.Name = schedule.Name;
        entity.Enabled = schedule.Enabled;
        entity.Type = schedule.Type.ToString();
        entity.TargetType = schedule.TargetType.ToString();
        entity.PresenceMode = schedule.PresenceMode?.ToString();
        entity.EnergyMode = schedule.EnergyMode?.ToString();
        entity.StartUtc = schedule.StartUtc;
        entity.EndUtc = schedule.EndUtc;
        entity.DaysOfWeekCsv = string.Join(',', schedule.DaysOfWeek.Select(x => (int)x));
        entity.DailyStartTime = schedule.DailyStartTime?.ToTimeSpan();
        entity.DailyEndTime = schedule.DailyEndTime?.ToTimeSpan();
        entity.Priority = schedule.Priority;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Schedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _dbContext.Schedules.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ScheduleDefinition ToDomain(ScheduleEntity entity)
    {
        var days = entity.DaysOfWeekCsv
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var dayValue) ? (DayOfWeek?)dayValue : null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToHashSet();

        return new ScheduleDefinition(
            entity.Id,
            entity.Name,
            entity.Enabled,
            ParseEnum(entity.Type, ScheduleType.OneTime),
            ParseEnum(entity.TargetType, ScheduleTargetType.Presence),
            ParseNullableEnum<PresenceMode>(entity.PresenceMode),
            ParseNullableEnum<EnergyMode>(entity.EnergyMode),
            entity.StartUtc,
            entity.EndUtc,
            days,
            entity.DailyStartTime.HasValue ? TimeOnly.FromTimeSpan(entity.DailyStartTime.Value) : null,
            entity.DailyEndTime.HasValue ? TimeOnly.FromTimeSpan(entity.DailyEndTime.Value) : null,
            entity.Priority);
    }

    private static ScheduleEntity ToEntity(ScheduleDefinition schedule)
    {
        return new ScheduleEntity
        {
            Id = schedule.Id,
            Name = schedule.Name,
            Enabled = schedule.Enabled,
            Type = schedule.Type.ToString(),
            TargetType = schedule.TargetType.ToString(),
            PresenceMode = schedule.PresenceMode?.ToString(),
            EnergyMode = schedule.EnergyMode?.ToString(),
            StartUtc = schedule.StartUtc,
            EndUtc = schedule.EndUtc,
            DaysOfWeekCsv = string.Join(',', schedule.DaysOfWeek.Select(x => (int)x)),
            DailyStartTime = schedule.DailyStartTime?.ToTimeSpan(),
            DailyEndTime = schedule.DailyEndTime?.ToTimeSpan(),
            Priority = schedule.Priority
        };
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : fallback;
    }

    private static TEnum? ParseNullableEnum<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : null;
    }
}
