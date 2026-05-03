namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class RoomEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClimateEntityId { get; set; } = string.Empty;
    public string? CurrentTemperatureEntityId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int PowerWatts { get; set; }
    public int Priority { get; set; }
    public bool CanStoreHeat { get; set; }
    public bool AllowPvBoost { get; set; }
    public bool AllowNightOverride { get; set; }
    public bool Enabled { get; set; }
    public int MinOnTimeSeconds { get; set; }
    public int MinOffTimeSeconds { get; set; }
    public int CommandCooldownSeconds { get; set; }

    public RoomPresetMappingEntity PresetMapping { get; set; } = null!;
}
