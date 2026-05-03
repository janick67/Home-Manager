using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record PowerManagerDecisionResult(
    DateTimeOffset EvaluatedAtUtc,
    EnergyMode EffectiveEnergyMode,
    PresenceMode EffectivePresenceMode,
    int AvailablePowerWatts,
    IReadOnlyCollection<RoomDecision> RoomDecisions);
