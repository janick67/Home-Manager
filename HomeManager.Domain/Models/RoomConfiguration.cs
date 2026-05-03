using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record RoomConfiguration(
    Guid Id,
    string Name,
    string ClimateEntityId,
    string? CurrentTemperatureEntityId,
    RoomType RoomType,
    int PowerWatts,
    int Priority,
    bool CanStoreHeat,
    bool AllowPvBoost,
    bool AllowNightOverride,
    bool Enabled,
    RoomPresetMapping Presets,
    int MinOnTimeSeconds,
    int MinOffTimeSeconds,
    int CommandCooldownSeconds);
