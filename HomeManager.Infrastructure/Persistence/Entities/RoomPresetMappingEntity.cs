namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class RoomPresetMappingEntity
{
    public Guid RoomId { get; set; }
    public string DefaultPreset { get; set; } = string.Empty;
    public string? EcoPreset { get; set; }
    public string? NightPreset { get; set; }
    public string? AwayPreset { get; set; }
    public string? StoragePreset { get; set; }
    public string? NoPowerPreset { get; set; }

    public RoomEntity Room { get; set; } = null!;
}
