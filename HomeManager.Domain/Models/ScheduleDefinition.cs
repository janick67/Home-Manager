using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record ScheduleDefinition(
    Guid Id,
    string Name,
    bool Enabled,
    ScheduleType Type,
    ScheduleTargetType TargetType,
    PresenceMode? PresenceMode,
    EnergyMode? EnergyMode,
    DateTimeOffset? StartUtc,
    DateTimeOffset? EndUtc,
    IReadOnlySet<DayOfWeek> DaysOfWeek,
    TimeOnly? DailyStartTime,
    TimeOnly? DailyEndTime,
    int Priority);
