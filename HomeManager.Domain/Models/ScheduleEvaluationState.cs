using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record ScheduleEvaluationState(
    bool IsNightMode,
    PresenceMode? ScheduledPresenceMode,
    EnergyMode? ScheduledEnergyMode,
    PresenceMode? ManualPresenceOverride,
    EnergyMode? ManualEnergyOverride);
