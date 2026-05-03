using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record ClimateEntityState(
    string EntityId,
    ThermostatPresetMode? CurrentPreset,
    IReadOnlySet<ThermostatPresetMode> SupportedPresets,
    DateTimeOffset? LastPresetCommandAtUtc,
    DateTimeOffset? LastStorageChangeAtUtc);
