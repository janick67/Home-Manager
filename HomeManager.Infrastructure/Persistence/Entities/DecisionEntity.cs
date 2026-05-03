namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class DecisionEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public string ManagerName { get; set; } = "PowerManager";
    public Guid RoomId { get; set; }
    public string ClimateEntityId { get; set; } = string.Empty;
    public string? PreviousPreset { get; set; }
    public string NewPreset { get; set; } = string.Empty;
    public string EnergyMode { get; set; } = string.Empty;
    public string PresenceMode { get; set; } = string.Empty;
    public int AvailablePowerWatts { get; set; }
    public int GridImportWatts { get; set; }
    public int GridExportWatts { get; set; }
    public int BatterySocPercent { get; set; }
    public int BatteryDischargeWatts { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool CommandSent { get; set; }
    public string? CommandResult { get; set; }
}
