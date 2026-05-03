namespace HomeManager.Application.Contracts;

public sealed record HomeAssistantEntityBindings(
    string? PvProductionEntityId,
    string? GridImportEntityId,
    string? GridExportEntityId,
    string? BatterySocEntityId,
    string? BatteryChargePowerEntityId,
    string? BatteryDischargePowerEntityId,
    IReadOnlyCollection<string> PresenceEntityIds);
