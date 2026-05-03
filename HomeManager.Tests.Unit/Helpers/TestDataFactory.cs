using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Tests.Unit.Helpers;

internal static class TestDataFactory
{
    public static PowerManagerSettings DefaultSettings()
    {
        return new PowerManagerSettings(
            SafetyMarginWatts: 500,
            MaxGridImportWatts: 200,
            MaxBatteryDischargeWatts: 100,
            MinBatterySocForStoragePercent: 40,
            EvaluationIntervalSeconds: 60,
            CommandCooldownSeconds: 60,
            HeartbeatIntervalSeconds: 45);
    }

    public static RoomConfiguration CreateRoom(
        string name,
        string climateEntityId,
        int powerWatts,
        int priority,
        RoomType roomType = RoomType.LivingRoom,
        bool canStoreHeat = true,
        bool allowPvBoost = true,
        bool enabled = true,
        ThermostatPresetMode defaultPreset = ThermostatPresetMode.Home,
        ThermostatPresetMode? ecoPreset = ThermostatPresetMode.Eco,
        ThermostatPresetMode? nightPreset = ThermostatPresetMode.Sleep,
        ThermostatPresetMode? awayPreset = ThermostatPresetMode.Away,
        ThermostatPresetMode? storagePreset = ThermostatPresetMode.Comfort,
        ThermostatPresetMode? noPowerPreset = ThermostatPresetMode.Eco,
        int minOnTimeSeconds = 0,
        int minOffTimeSeconds = 0,
        int commandCooldownSeconds = 60)
    {
        return new RoomConfiguration(
            Guid.NewGuid(),
            name,
            climateEntityId,
            null,
            roomType,
            powerWatts,
            priority,
            canStoreHeat,
            allowPvBoost,
            AllowNightOverride: true,
            enabled,
            new RoomPresetMapping(
                defaultPreset,
                ecoPreset,
                nightPreset,
                awayPreset,
                storagePreset,
                noPowerPreset),
            minOnTimeSeconds,
            minOffTimeSeconds,
            commandCooldownSeconds);
    }

    public static ClimateEntityState CreateClimateState(
        string entityId,
        ThermostatPresetMode? currentPreset,
        IReadOnlySet<ThermostatPresetMode>? supportedPresets = null,
        DateTimeOffset? lastPresetCommandAtUtc = null,
        DateTimeOffset? lastStorageChangeAtUtc = null)
    {
        return new ClimateEntityState(
            entityId,
            currentPreset,
            supportedPresets ?? new HashSet<ThermostatPresetMode>
            {
                ThermostatPresetMode.None,
                ThermostatPresetMode.Away,
                ThermostatPresetMode.Comfort,
                ThermostatPresetMode.Eco,
                ThermostatPresetMode.Home,
                ThermostatPresetMode.Sleep,
                ThermostatPresetMode.Activity
            },
            lastPresetCommandAtUtc,
            lastStorageChangeAtUtc);
    }
}
