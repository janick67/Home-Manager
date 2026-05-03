using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record ManualOverrideDefinition(
    Guid Id,
    string Name,
    bool Enabled,
    ScheduleTargetType TargetType,
    PresenceMode? PresenceMode,
    EnergyMode? EnergyMode,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    int Priority);
