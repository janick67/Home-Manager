using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record PowerManagerInput(
    IReadOnlyCollection<RoomConfiguration> Rooms,
    IReadOnlyDictionary<string, ClimateEntityState> ClimateStates,
    EnergyState EnergyState,
    EnergyMode EnergyMode,
    PresenceMode PresenceMode,
    ScheduleEvaluationState ScheduleState,
    PowerManagerSettings Settings);
