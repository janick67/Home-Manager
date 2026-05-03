using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Api.Models;

public sealed record DashboardDto(
    bool HomeAssistantConnected,
    EnergyMode EnergyMode,
    PresenceMode PresenceMode,
    int PvProductionWatts,
    int GridImportWatts,
    int GridExportWatts,
    int BatterySocPercent,
    int BatteryChargeWatts,
    int BatteryDischargeWatts,
    int AvailablePowerWatts,
    int ActiveSchedules,
    int ActiveOverrides,
    IReadOnlyCollection<PowerManagerDecisionResult> LatestDecisions);
