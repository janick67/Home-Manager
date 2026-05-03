namespace HomeManager.Domain.Models;

public sealed record EnergyState(
    int PvProductionWatts,
    int GridImportWatts,
    int GridExportWatts,
    int BatterySocPercent,
    int BatteryChargeWatts,
    int BatteryDischargeWatts);
