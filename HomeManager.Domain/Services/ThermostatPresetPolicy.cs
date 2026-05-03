using System.Diagnostics.CodeAnalysis;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Domain.Services;

public sealed class ThermostatPresetPolicy
{
    public PresetSelectionResult SelectSupportedPreset(
        ThermostatPresetMode desiredPreset,
        RoomConfiguration room,
        ClimateEntityState climateState)
    {
        var fallbackChain = room.Presets.BuildFallbackChain(desiredPreset);
        foreach (var candidate in fallbackChain)
        {
            if (IsPresetSupported(candidate, climateState.SupportedPresets))
            {
                var fallbackUsed = candidate != desiredPreset;
                var reason = fallbackUsed
                    ? $"Preset {desiredPreset} unsupported, fallback {candidate} selected."
                    : $"Preset {desiredPreset} supported.";

                return new PresetSelectionResult(candidate, fallbackUsed, reason);
            }
        }

        var safePreset = climateState.CurrentPreset ?? room.Presets.DefaultPreset;
        return new PresetSelectionResult(
            safePreset,
            true,
            "No supported preset from fallback chain; keeping current or default preset.");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Kept as instance method for injectable policy service consistency.")]
    public bool IsPresetSupported(
        ThermostatPresetMode preset,
        IReadOnlySet<ThermostatPresetMode> supportedPresets)
    {
        if (supportedPresets.Count == 0)
        {
            return true;
        }

        if (preset == ThermostatPresetMode.None && supportedPresets.Contains(ThermostatPresetMode.None))
        {
            return true;
        }

        return supportedPresets.Contains(preset);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Kept as instance method for injectable policy service consistency.")]
    public string ToHomeAssistantPresetValue(ThermostatPresetMode presetMode)
    {
        return presetMode switch
        {
            ThermostatPresetMode.None => "none",
            ThermostatPresetMode.Away => "away",
            ThermostatPresetMode.Comfort => "comfort",
            ThermostatPresetMode.Eco => "eco",
            ThermostatPresetMode.Home => "home",
            ThermostatPresetMode.Sleep => "sleep",
            ThermostatPresetMode.Activity => "activity",
            _ => "none"
        };
    }
}
