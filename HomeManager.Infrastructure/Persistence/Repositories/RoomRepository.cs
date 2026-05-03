using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class RoomRepository : IRoomRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public RoomRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<RoomConfiguration>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Rooms
            .Include(x => x.PresetMapping)
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomain).ToArray();
    }

    public async Task<RoomConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Rooms
            .Include(x => x.PresetMapping)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task AddAsync(RoomConfiguration room, CancellationToken cancellationToken)
    {
        var entity = ToEntity(room);
        _dbContext.Rooms.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RoomConfiguration room, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Rooms
            .Include(x => x.PresetMapping)
            .FirstOrDefaultAsync(x => x.Id == room.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Room {room.Id} not found.");

        entity.Name = room.Name;
        entity.ClimateEntityId = room.ClimateEntityId;
        entity.CurrentTemperatureEntityId = room.CurrentTemperatureEntityId;
        entity.RoomType = room.RoomType.ToString();
        entity.PowerWatts = room.PowerWatts;
        entity.Priority = room.Priority;
        entity.CanStoreHeat = room.CanStoreHeat;
        entity.AllowPvBoost = room.AllowPvBoost;
        entity.AllowNightOverride = room.AllowNightOverride;
        entity.Enabled = room.Enabled;
        entity.MinOnTimeSeconds = room.MinOnTimeSeconds;
        entity.MinOffTimeSeconds = room.MinOffTimeSeconds;
        entity.CommandCooldownSeconds = room.CommandCooldownSeconds;

        entity.PresetMapping.DefaultPreset = room.Presets.DefaultPreset.ToString();
        entity.PresetMapping.EcoPreset = room.Presets.EcoPreset?.ToString();
        entity.PresetMapping.NightPreset = room.Presets.NightPreset?.ToString();
        entity.PresetMapping.AwayPreset = room.Presets.AwayPreset?.ToString();
        entity.PresetMapping.StoragePreset = room.Presets.StoragePreset?.ToString();
        entity.PresetMapping.NoPowerPreset = room.Presets.NoPowerPreset?.ToString();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Rooms
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _dbContext.Rooms.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static RoomConfiguration ToDomain(RoomEntity entity)
    {
        var mapping = entity.PresetMapping;
        return new RoomConfiguration(
            entity.Id,
            entity.Name,
            entity.ClimateEntityId,
            entity.CurrentTemperatureEntityId,
            ParseEnum(entity.RoomType, RoomType.LivingRoom),
            entity.PowerWatts,
            entity.Priority,
            entity.CanStoreHeat,
            entity.AllowPvBoost,
            entity.AllowNightOverride,
            entity.Enabled,
            new RoomPresetMapping(
                ParseEnum(mapping.DefaultPreset, ThermostatPresetMode.Home),
                ParseNullablePreset(mapping.EcoPreset),
                ParseNullablePreset(mapping.NightPreset),
                ParseNullablePreset(mapping.AwayPreset),
                ParseNullablePreset(mapping.StoragePreset),
                ParseNullablePreset(mapping.NoPowerPreset)),
            entity.MinOnTimeSeconds,
            entity.MinOffTimeSeconds,
            entity.CommandCooldownSeconds);
    }

    private static RoomEntity ToEntity(RoomConfiguration room)
    {
        return new RoomEntity
        {
            Id = room.Id,
            Name = room.Name,
            ClimateEntityId = room.ClimateEntityId,
            CurrentTemperatureEntityId = room.CurrentTemperatureEntityId,
            RoomType = room.RoomType.ToString(),
            PowerWatts = room.PowerWatts,
            Priority = room.Priority,
            CanStoreHeat = room.CanStoreHeat,
            AllowPvBoost = room.AllowPvBoost,
            AllowNightOverride = room.AllowNightOverride,
            Enabled = room.Enabled,
            MinOnTimeSeconds = room.MinOnTimeSeconds,
            MinOffTimeSeconds = room.MinOffTimeSeconds,
            CommandCooldownSeconds = room.CommandCooldownSeconds,
            PresetMapping = new RoomPresetMappingEntity
            {
                RoomId = room.Id,
                DefaultPreset = room.Presets.DefaultPreset.ToString(),
                EcoPreset = room.Presets.EcoPreset?.ToString(),
                NightPreset = room.Presets.NightPreset?.ToString(),
                AwayPreset = room.Presets.AwayPreset?.ToString(),
                StoragePreset = room.Presets.StoragePreset?.ToString(),
                NoPowerPreset = room.Presets.NoPowerPreset?.ToString()
            }
        };
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : fallback;
    }

    private static ThermostatPresetMode? ParseNullablePreset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ThermostatPresetMode>(value, true, out var parsed)
            ? parsed
            : null;
    }
}
