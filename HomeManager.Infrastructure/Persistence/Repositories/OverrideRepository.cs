using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class OverrideRepository : IOverrideRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public OverrideRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ManualOverrideDefinition>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Overrides
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.StartsAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomain).ToArray();
    }

    public async Task AddAsync(ManualOverrideDefinition entry, CancellationToken cancellationToken)
    {
        _dbContext.Overrides.Add(new OverrideEntity
        {
            Id = entry.Id,
            Name = entry.Name,
            Enabled = entry.Enabled,
            TargetType = entry.TargetType.ToString(),
            PresenceMode = entry.PresenceMode?.ToString(),
            EnergyMode = entry.EnergyMode?.ToString(),
            StartsAtUtc = entry.StartsAtUtc,
            EndsAtUtc = entry.EndsAtUtc,
            Priority = entry.Priority
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Overrides.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _dbContext.Overrides.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ManualOverrideDefinition ToDomain(OverrideEntity entity)
    {
        return new ManualOverrideDefinition(
            entity.Id,
            entity.Name,
            entity.Enabled,
            ParseEnum(entity.TargetType, ScheduleTargetType.Presence),
            ParseNullableEnum<PresenceMode>(entity.PresenceMode),
            ParseNullableEnum<EnergyMode>(entity.EnergyMode),
            entity.StartsAtUtc,
            entity.EndsAtUtc,
            entity.Priority);
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
