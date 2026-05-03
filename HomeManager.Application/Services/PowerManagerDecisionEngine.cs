using HomeManager.Application.Abstractions;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Domain.Services;

namespace HomeManager.Application.Services;

public sealed class PowerManagerDecisionEngine : IPowerManagerDecisionEngine
{
    private readonly IClock _clock;
    private readonly ThermostatPresetPolicy _presetPolicy;

    public PowerManagerDecisionEngine(IClock clock, ThermostatPresetPolicy presetPolicy)
    {
        _clock = clock;
        _presetPolicy = presetPolicy;
    }

    public PowerManagerDecisionResult Evaluate(PowerManagerInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var now = _clock.UtcNow;
        var effectiveEnergyMode = ResolveEffectiveEnergyMode(input);
        var effectivePresenceMode = ResolveEffectivePresenceMode(input);
        var availablePower = input.EnergyState.GridExportWatts - input.Settings.SafetyMarginWatts;

        var roomPlans = input.Rooms.ToDictionary(
            room => room.Id,
            room => new RoomPlan(
                DetermineBasePreset(room, effectiveEnergyMode, effectivePresenceMode, input.ScheduleState),
                new List<string> { "Base preset calculated." }));

        RetainExistingStorageIfEligible(input, effectiveEnergyMode, roomPlans);
        ApplyOverloadStorageShedding(input, effectiveEnergyMode, effectivePresenceMode, roomPlans);
        ApplyStorageBudgetAndBoost(input, effectiveEnergyMode, effectivePresenceMode, availablePower, roomPlans);

        var roomDecisions = new List<RoomDecision>();
        foreach (var room in input.Rooms.OrderBy(x => x.Priority).ThenBy(x => x.Name))
        {
            var climateState = input.ClimateStates.TryGetValue(room.ClimateEntityId, out var state)
                ? state
                : new ClimateEntityState(
                    room.ClimateEntityId,
                    null,
                    new HashSet<ThermostatPresetMode>(),
                    null,
                    null);

            var plan = roomPlans[room.Id];
            if (!room.Enabled)
            {
                plan.Reasons.Add("Room disabled, command skipped.");
                var disabledTarget = climateState.CurrentPreset ?? room.Presets.DefaultPreset;
                roomDecisions.Add(new RoomDecision(
                    room.Id,
                    room.Name,
                    room.ClimateEntityId,
                    climateState.CurrentPreset,
                    disabledTarget,
                    new DecisionReason("RoomDisabled", string.Join(" ", plan.Reasons.Distinct())),
                    false,
                    null));
                continue;
            }

            var selectedPreset = _presetPolicy.SelectSupportedPreset(plan.TargetPreset, room, climateState);
            plan.Reasons.Add(selectedPreset.Reason);

            var guardedPreset = ApplySwitchingTimeGuards(
                room,
                climateState,
                selectedPreset.AppliedPreset,
                now,
                plan.Reasons);

            var command = BuildCommandIfAllowed(
                room,
                climateState,
                guardedPreset,
                now,
                input.Settings.CommandCooldownSeconds,
                plan.Reasons);

            roomDecisions.Add(new RoomDecision(
                room.Id,
                room.Name,
                room.ClimateEntityId,
                climateState.CurrentPreset,
                guardedPreset,
                new DecisionReason("PowerManagerDecision", string.Join(" ", plan.Reasons.Distinct())),
                command is not null,
                command));
        }

        return new PowerManagerDecisionResult(
            now,
            effectiveEnergyMode,
            effectivePresenceMode,
            availablePower,
            roomDecisions);
    }

    private static ThermostatPresetMode DetermineBasePreset(
        RoomConfiguration room,
        EnergyMode energyMode,
        PresenceMode presenceMode,
        ScheduleEvaluationState scheduleState)
    {
        if (energyMode == EnergyMode.NoGridPower)
        {
            return room.Presets.NoPowerPreset
                ?? room.Presets.EcoPreset
                ?? ThermostatPresetMode.None;
        }

        if (presenceMode == PresenceMode.Vacation)
        {
            return room.Presets.AwayPreset
                ?? room.Presets.EcoPreset
                ?? ThermostatPresetMode.None;
        }

        if (scheduleState.IsNightMode && room.RoomType == RoomType.Bedroom)
        {
            return room.Presets.NightPreset ?? ThermostatPresetMode.Sleep;
        }

        if (presenceMode == PresenceMode.NobodyHome)
        {
            return room.Presets.AwayPreset
                ?? room.Presets.EcoPreset
                ?? room.Presets.DefaultPreset;
        }

        if (energyMode == EnergyMode.ExpensivePower)
        {
            return room.Presets.EcoPreset ?? room.Presets.DefaultPreset;
        }

        return room.Presets.DefaultPreset;
    }

    private static EnergyMode ResolveEffectiveEnergyMode(PowerManagerInput input)
    {
        if (input.EnergyMode == EnergyMode.NoGridPower)
        {
            return EnergyMode.NoGridPower;
        }

        if (input.ScheduleState.ManualEnergyOverride.HasValue)
        {
            return input.ScheduleState.ManualEnergyOverride.Value;
        }

        if (input.ScheduleState.ScheduledEnergyMode.HasValue)
        {
            return input.ScheduleState.ScheduledEnergyMode.Value;
        }

        return input.EnergyMode;
    }

    private static PresenceMode ResolveEffectivePresenceMode(PowerManagerInput input)
    {
        var candidates = new List<PresenceMode> { input.PresenceMode };
        if (input.ScheduleState.ScheduledPresenceMode.HasValue)
        {
            candidates.Add(input.ScheduleState.ScheduledPresenceMode.Value);
        }

        if (input.ScheduleState.ManualPresenceOverride.HasValue)
        {
            candidates.Add(input.ScheduleState.ManualPresenceOverride.Value);
        }

        if (candidates.Contains(PresenceMode.Vacation))
        {
            return PresenceMode.Vacation;
        }

        if (input.ScheduleState.ManualPresenceOverride.HasValue)
        {
            return input.ScheduleState.ManualPresenceOverride.Value;
        }

        if (input.ScheduleState.ScheduledPresenceMode.HasValue)
        {
            return input.ScheduleState.ScheduledPresenceMode.Value;
        }

        return input.PresenceMode;
    }

    private static void RetainExistingStorageIfEligible(
        PowerManagerInput input,
        EnergyMode effectiveEnergyMode,
        IReadOnlyDictionary<Guid, RoomPlan> roomPlans)
    {
        foreach (var room in input.Rooms)
        {
            if (!CanUseStorage(room, input, effectiveEnergyMode))
            {
                continue;
            }

            if (!input.ClimateStates.TryGetValue(room.ClimateEntityId, out var climateState))
            {
                continue;
            }

            if (!room.Presets.StoragePreset.HasValue || climateState.CurrentPreset != room.Presets.StoragePreset.Value)
            {
                continue;
            }

            var plan = roomPlans[room.Id];
            plan.TargetPreset = room.Presets.StoragePreset.Value;
            plan.Reasons.Add("Existing storage state retained.");
        }
    }

    private static void ApplyStorageBudgetAndBoost(
        PowerManagerInput input,
        EnergyMode effectiveEnergyMode,
        PresenceMode effectivePresenceMode,
        int availablePower,
        IReadOnlyDictionary<Guid, RoomPlan> roomPlans)
    {
        if (effectiveEnergyMode is EnergyMode.NoGridPower or EnergyMode.ExpensivePower)
        {
            DisableStorageForAllPlans(
                input,
                effectiveEnergyMode,
                effectivePresenceMode,
                roomPlans,
                "Storage disabled due to restrictive energy mode.");
            return;
        }

        if (input.EnergyState.BatterySocPercent < input.Settings.MinBatterySocForStoragePercent)
        {
            DisableStorageForAllPlans(
                input,
                effectiveEnergyMode,
                effectivePresenceMode,
                roomPlans,
                "Storage blocked because battery SOC is below threshold.");
            return;
        }

        var importOverLimit = input.EnergyState.GridImportWatts > input.Settings.MaxGridImportWatts;
        var dischargeOverLimit = input.EnergyState.BatteryDischargeWatts > input.Settings.MaxBatteryDischargeWatts;

        var storagePlannedRooms = input.Rooms
            .Where(room => room.Presets.StoragePreset.HasValue
                && roomPlans[room.Id].TargetPreset == room.Presets.StoragePreset.Value)
            .OrderByDescending(room => room.Priority)
            .ThenBy(room => room.Name)
            .ToList();
        var consumedByPlannedStorage = storagePlannedRooms.Sum(x => x.PowerWatts);
        var remainingForShedding = availablePower - consumedByPlannedStorage;
        foreach (var room in storagePlannedRooms)
        {
            if (remainingForShedding >= 0)
            {
                break;
            }

            var plan = roomPlans[room.Id];
            plan.TargetPreset = DetermineStorageFallbackPreset(room, effectiveEnergyMode, effectivePresenceMode);
            plan.Reasons.Add("Storage revoked to fit current power budget.");
            remainingForShedding += room.PowerWatts;
        }

        if (availablePower <= 0 || importOverLimit || dischargeOverLimit)
        {
            return;
        }

        var updatedStorageConsumption = input.Rooms
            .Where(room => room.Presets.StoragePreset.HasValue
                && roomPlans[room.Id].TargetPreset == room.Presets.StoragePreset.Value)
            .Sum(room => room.PowerWatts);

        var remainingForBoost = (availablePower + input.Settings.SafetyMarginWatts) - updatedStorageConsumption;
        if (remainingForBoost <= 0)
        {
            return;
        }

        var candidates = input.Rooms
            .Where(room => CanUseStorage(room, input, effectiveEnergyMode))
            .Where(room => room.Presets.StoragePreset.HasValue
                && roomPlans[room.Id].TargetPreset != room.Presets.StoragePreset.Value)
            .OrderBy(room => room.Priority)
            .ThenBy(room => room.Name);

        foreach (var room in candidates)
        {
            if (room.PowerWatts > remainingForBoost)
            {
                continue;
            }

            var plan = roomPlans[room.Id];
            plan.TargetPreset = room.Presets.StoragePreset!.Value;
            plan.Reasons.Add($"Storage boost enabled within power budget ({remainingForBoost}W available).");
            remainingForBoost -= room.PowerWatts;
        }
    }

    private static void DisableStorageForAllPlans(
        PowerManagerInput input,
        EnergyMode effectiveEnergyMode,
        PresenceMode effectivePresenceMode,
        IReadOnlyDictionary<Guid, RoomPlan> roomPlans,
        string reason)
    {
        foreach (var room in input.Rooms)
        {
            if (!room.Presets.StoragePreset.HasValue)
            {
                continue;
            }

            var plan = roomPlans[room.Id];
            if (plan.TargetPreset != room.Presets.StoragePreset.Value)
            {
                continue;
            }

            plan.TargetPreset = DetermineStorageFallbackPreset(room, effectiveEnergyMode, effectivePresenceMode);
            plan.Reasons.Add(reason);
        }
    }

    private static void ApplyOverloadStorageShedding(
        PowerManagerInput input,
        EnergyMode effectiveEnergyMode,
        PresenceMode effectivePresenceMode,
        IReadOnlyDictionary<Guid, RoomPlan> roomPlans)
    {
        var importOverLimit = input.EnergyState.GridImportWatts > input.Settings.MaxGridImportWatts;
        var dischargeOverLimit = input.EnergyState.BatteryDischargeWatts > input.Settings.MaxBatteryDischargeWatts;

        if (!importOverLimit && !dischargeOverLimit)
        {
            return;
        }

        var storageActiveRooms = input.Rooms
            .Where(room => room.Presets.StoragePreset.HasValue
                && roomPlans[room.Id].TargetPreset == room.Presets.StoragePreset.Value)
            .OrderByDescending(room => room.Priority)
            .ThenBy(room => room.Name)
            .ToList();

        if (storageActiveRooms.Count == 0)
        {
            storageActiveRooms = input.Rooms
                .Where(room => room.Presets.StoragePreset.HasValue
                    && input.ClimateStates.TryGetValue(room.ClimateEntityId, out var climate)
                    && climate.CurrentPreset == room.Presets.StoragePreset.Value)
                .OrderByDescending(room => room.Priority)
                .ThenBy(room => room.Name)
                .ToList();
        }

        var roomToShed = storageActiveRooms.FirstOrDefault();
        if (roomToShed is null)
        {
            return;
        }

        var plan = roomPlans[roomToShed.Id];
        plan.TargetPreset = DetermineStorageFallbackPreset(roomToShed, effectiveEnergyMode, effectivePresenceMode);
        plan.Reasons.Add("Storage revoked due to grid import/battery discharge safety limits.");
    }

    private static bool CanUseStorage(
        RoomConfiguration room,
        PowerManagerInput input,
        EnergyMode effectiveEnergyMode)
    {
        if (!room.Enabled || !room.CanStoreHeat || !room.AllowPvBoost || !room.Presets.StoragePreset.HasValue)
        {
            return false;
        }

        if (effectiveEnergyMode is EnergyMode.NoGridPower or EnergyMode.ExpensivePower)
        {
            return false;
        }

        if (input.EnergyState.BatterySocPercent < input.Settings.MinBatterySocForStoragePercent)
        {
            return false;
        }

        if (room.RoomType == RoomType.Bedroom && input.ScheduleState.IsNightMode)
        {
            return false;
        }

        return true;
    }

    private static ThermostatPresetMode DetermineStorageFallbackPreset(
        RoomConfiguration room,
        EnergyMode energyMode,
        PresenceMode presenceMode)
    {
        if (energyMode == EnergyMode.NoGridPower)
        {
            return room.Presets.NoPowerPreset
                ?? room.Presets.EcoPreset
                ?? ThermostatPresetMode.None;
        }

        if (presenceMode == PresenceMode.Vacation || presenceMode == PresenceMode.NobodyHome)
        {
            return room.Presets.AwayPreset
                ?? room.Presets.EcoPreset
                ?? room.Presets.DefaultPreset;
        }

        if (energyMode == EnergyMode.ExpensivePower)
        {
            return room.Presets.EcoPreset ?? room.Presets.DefaultPreset;
        }

        return room.Presets.DefaultPreset;
    }

    private static ThermostatPresetMode ApplySwitchingTimeGuards(
        RoomConfiguration room,
        ClimateEntityState climateState,
        ThermostatPresetMode targetPreset,
        DateTimeOffset now,
        List<string> reasons)
    {
        if (!room.Presets.StoragePreset.HasValue)
        {
            return targetPreset;
        }

        var storagePreset = room.Presets.StoragePreset.Value;
        var currentlyStorage = climateState.CurrentPreset == storagePreset;
        var targetStorage = targetPreset == storagePreset;

        if (currentlyStorage && !targetStorage && climateState.LastStorageChangeAtUtc.HasValue)
        {
            var minimumOnWindow = TimeSpan.FromSeconds(room.MinOnTimeSeconds);
            if (now - climateState.LastStorageChangeAtUtc.Value < minimumOnWindow && climateState.CurrentPreset.HasValue)
            {
                reasons.Add("MinOnTime active, keeping storage preset.");
                return climateState.CurrentPreset.Value;
            }
        }

        if (!currentlyStorage && targetStorage && climateState.LastStorageChangeAtUtc.HasValue)
        {
            var minimumOffWindow = TimeSpan.FromSeconds(room.MinOffTimeSeconds);
            if (now - climateState.LastStorageChangeAtUtc.Value < minimumOffWindow)
            {
                reasons.Add("MinOffTime active, storage start blocked.");
                return climateState.CurrentPreset ?? room.Presets.DefaultPreset;
            }
        }

        return targetPreset;
    }

    private HaCommandRequest? BuildCommandIfAllowed(
        RoomConfiguration room,
        ClimateEntityState climateState,
        ThermostatPresetMode targetPreset,
        DateTimeOffset now,
        int globalCommandCooldownSeconds,
        List<string> reasons)
    {
        if (climateState.CurrentPreset.HasValue && climateState.CurrentPreset.Value == targetPreset)
        {
            reasons.Add("Preset already set, command skipped.");
            return null;
        }

        var cooldownSeconds = room.CommandCooldownSeconds > 0
            ? room.CommandCooldownSeconds
            : globalCommandCooldownSeconds;

        if (cooldownSeconds > 0 && climateState.LastPresetCommandAtUtc.HasValue)
        {
            var cooldown = TimeSpan.FromSeconds(cooldownSeconds);
            var elapsed = now - climateState.LastPresetCommandAtUtc.Value;
            if (elapsed < cooldown)
            {
                reasons.Add("Command cooldown active, command skipped.");
                return null;
            }
        }

        if (!_presetPolicy.IsPresetSupported(targetPreset, climateState.SupportedPresets))
        {
            reasons.Add("Target preset unsupported by climate entity, command skipped.");
            return null;
        }

        var serviceData = new Dictionary<string, object?>
        {
            ["entity_id"] = room.ClimateEntityId,
            ["preset_mode"] = _presetPolicy.ToHomeAssistantPresetValue(targetPreset)
        };

        reasons.Add("Command prepared for Home Assistant.");
        return new HaCommandRequest("climate", "set_preset_mode", room.ClimateEntityId, serviceData);
    }

    private sealed class RoomPlan
    {
        public RoomPlan(ThermostatPresetMode targetPreset, List<string> reasons)
        {
            TargetPreset = targetPreset;
            Reasons = reasons;
        }

        public ThermostatPresetMode TargetPreset { get; set; }
        public List<string> Reasons { get; }
    }
}