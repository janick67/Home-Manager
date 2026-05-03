namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class SettingEntity
{
    public int Id { get; set; }
    public string HomeAssistantBaseUrl { get; set; } = string.Empty;
    public string HomeAssistantToken { get; set; } = string.Empty;
    public int SafetyMarginWatts { get; set; } = 500;
    public int MaxGridImportWatts { get; set; } = 200;
    public int MaxBatteryDischargeWatts { get; set; } = 100;
    public int MinBatterySocForStoragePercent { get; set; } = 40;
    public int EvaluationIntervalSeconds { get; set; } = 60;
    public int CommandCooldownSeconds { get; set; } = 60;
    public int HeartbeatIntervalSeconds { get; set; } = 45;
    public string? PvProductionEntityId { get; set; }
    public string? GridImportEntityId { get; set; }
    public string? GridExportEntityId { get; set; }
    public string? BatterySocEntityId { get; set; }
    public string? BatteryChargePowerEntityId { get; set; }
    public string? BatteryDischargePowerEntityId { get; set; }
    public string PresenceEntityIdsCsv { get; set; } = string.Empty;
}
