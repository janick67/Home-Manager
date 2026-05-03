using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record RoomDecision(
    Guid RoomId,
    string RoomName,
    string ClimateEntityId,
    ThermostatPresetMode? PreviousPreset,
    ThermostatPresetMode TargetPreset,
    DecisionReason Reason,
    bool CommandSent,
    HaCommandRequest? CommandRequest);
