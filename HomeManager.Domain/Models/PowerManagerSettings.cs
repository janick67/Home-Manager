namespace HomeManager.Domain.Models;

public sealed record PowerManagerSettings(
    int SafetyMarginWatts,
    int MaxGridImportWatts,
    int MaxBatteryDischargeWatts,
    int MinBatterySocForStoragePercent,
    int EvaluationIntervalSeconds,
    int CommandCooldownSeconds,
    int HeartbeatIntervalSeconds);
