namespace HomeManager.Api.Models;

public sealed record SettingsDto(
    string HomeAssistantBaseUrl,
    string HomeAssistantToken,
    int SafetyMarginWatts,
    int MaxGridImportWatts,
    int MaxBatteryDischargeWatts,
    int MinBatterySocForStoragePercent,
    int EvaluationIntervalSeconds,
    int CommandCooldownSeconds,
    int HeartbeatIntervalSeconds,
    string? PvProductionEntityId,
    string? GridImportEntityId,
    string? GridExportEntityId,
    string? BatterySocEntityId,
    string? BatteryChargePowerEntityId,
    string? BatteryDischargePowerEntityId,
    IReadOnlyCollection<string> PresenceEntityIds);
