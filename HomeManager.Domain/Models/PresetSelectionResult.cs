using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record PresetSelectionResult(
    ThermostatPresetMode AppliedPreset,
    bool FallbackUsed,
    string Reason);
