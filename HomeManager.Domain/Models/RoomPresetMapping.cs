using HomeManager.Domain.Enums;

namespace HomeManager.Domain.Models;

public sealed record RoomPresetMapping(
    ThermostatPresetMode DefaultPreset,
    ThermostatPresetMode? EcoPreset,
    ThermostatPresetMode? NightPreset,
    ThermostatPresetMode? AwayPreset,
    ThermostatPresetMode? StoragePreset,
    ThermostatPresetMode? NoPowerPreset)
{
    public IReadOnlyList<ThermostatPresetMode> BuildFallbackChain(ThermostatPresetMode preferred)
    {
        var candidates = new List<ThermostatPresetMode>
        {
            preferred,
            DefaultPreset
        };

        if (EcoPreset.HasValue)
        {
            candidates.Add(EcoPreset.Value);
        }

        if (AwayPreset.HasValue)
        {
            candidates.Add(AwayPreset.Value);
        }

        if (NoPowerPreset.HasValue)
        {
            candidates.Add(NoPowerPreset.Value);
        }

        candidates.Add(ThermostatPresetMode.None);
        return candidates.Distinct().ToArray();
    }
}
