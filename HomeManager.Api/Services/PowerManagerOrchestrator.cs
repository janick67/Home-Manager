using HomeManager.Application.Abstractions;
using HomeManager.Application.Ports.HomeAssistant;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Application.Services;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HomeManager.Api.Services;

public sealed partial class PowerManagerOrchestrator
{
    private const string ManagerName = "PowerManager";
    private readonly IClock _clock;
    private readonly IHomeAssistantStateReader _stateReader;
    private readonly IHomeAssistantCommandSender _commandSender;
    private readonly IRoomRepository _roomRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IOverrideRepository _overrideRepository;
    private readonly IScheduleEvaluator _scheduleEvaluator;
    private readonly IPowerManagerDecisionEngine _decisionEngine;
    private readonly IDecisionHistoryRepository _decisionHistoryRepository;
    private readonly IHaCommandHistoryRepository _haCommandHistoryRepository;
    private readonly IManagerStateRepository _managerStateRepository;
    private readonly ILogger<PowerManagerOrchestrator> _logger;

    public PowerManagerOrchestrator(
        IClock clock,
        IHomeAssistantStateReader stateReader,
        IHomeAssistantCommandSender commandSender,
        IRoomRepository roomRepository,
        ISettingsRepository settingsRepository,
        IScheduleRepository scheduleRepository,
        IOverrideRepository overrideRepository,
        IScheduleEvaluator scheduleEvaluator,
        IPowerManagerDecisionEngine decisionEngine,
        IDecisionHistoryRepository decisionHistoryRepository,
        IHaCommandHistoryRepository haCommandHistoryRepository,
        IManagerStateRepository managerStateRepository,
        ILogger<PowerManagerOrchestrator> logger)
    {
        _clock = clock;
        _stateReader = stateReader;
        _commandSender = commandSender;
        _roomRepository = roomRepository;
        _settingsRepository = settingsRepository;
        _scheduleRepository = scheduleRepository;
        _overrideRepository = overrideRepository;
        _scheduleEvaluator = scheduleEvaluator;
        _decisionEngine = decisionEngine;
        _decisionHistoryRepository = decisionHistoryRepository;
        _haCommandHistoryRepository = haCommandHistoryRepository;
        _managerStateRepository = managerStateRepository;
        _logger = logger;
    }

    public async Task<PowerManagerDecisionResult> EvaluateAsync(bool sendCommands, CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.GetAllAsync(cancellationToken);
        var settings = await _settingsRepository.GetPowerManagerSettingsAsync(cancellationToken);
        var bindings = await _settingsRepository.GetEntityBindingsAsync(cancellationToken);
        var schedules = await _scheduleRepository.GetAllAsync(cancellationToken);
        var overrides = await _overrideRepository.GetAllAsync(cancellationToken);

        var scheduleState = _scheduleEvaluator.Evaluate(
            schedules,
            overrides,
            IsNightMode(_clock.UtcNow));

        var climateStates = new Dictionary<string, ClimateEntityState>(StringComparer.OrdinalIgnoreCase);
        foreach (var room in rooms)
        {
            var entity = await _stateReader.GetStateAsync(room.ClimateEntityId, cancellationToken);
            climateStates[room.ClimateEntityId] = BuildClimateState(room.ClimateEntityId, entity);
        }

        var energyState = await BuildEnergyStateAsync(bindings, cancellationToken);
        var energyMode = ResolveEnergyMode(energyState, settings);
        var presenceMode = await ResolvePresenceModeAsync(bindings, cancellationToken);

        var input = new PowerManagerInput(
            rooms,
            climateStates,
            energyState,
            energyMode,
            presenceMode,
            scheduleState,
            settings);

        var result = _decisionEngine.Evaluate(input);
        if (sendCommands)
        {
            await DispatchCommandsAsync(result, cancellationToken);
        }

        await _decisionHistoryRepository.AddAsync(result, cancellationToken);
        await _managerStateRepository.SetLastEvaluationAsync(ManagerName, _clock.UtcNow, cancellationToken);
        return result;
    }

    private async Task DispatchCommandsAsync(PowerManagerDecisionResult result, CancellationToken cancellationToken)
    {
        foreach (var decision in result.RoomDecisions.Where(x => x.CommandSent && x.CommandRequest is not null))
        {
            try
            {
                var command = decision.CommandRequest!;
                var response = await _commandSender.CallServiceAsync(
                    command.Domain,
                    command.Service,
                    command.Data,
                    cancellationToken);

                var commandResultMessage = response.Success ? "OK" : $"FAILED: {response.Message}";
                await _haCommandHistoryRepository.AddAsync(decision, commandResultMessage, cancellationToken);
            }
            catch (Exception exception)
            {
                LogDispatchFailure(_logger, decision.ClimateEntityId, exception);

                await _haCommandHistoryRepository.AddAsync(decision, $"FAILED: {exception.Message}", cancellationToken);
            }
        }
    }

    private async Task<EnergyState> BuildEnergyStateAsync(
        Application.Contracts.HomeAssistantEntityBindings bindings,
        CancellationToken cancellationToken)
    {
        var pvProduction = await ReadNumericStateAsync(bindings.PvProductionEntityId, cancellationToken);
        var gridImport = await ReadNumericStateAsync(bindings.GridImportEntityId, cancellationToken);
        var gridExport = await ReadNumericStateAsync(bindings.GridExportEntityId, cancellationToken);
        var batterySoc = await ReadNumericStateAsync(bindings.BatterySocEntityId, cancellationToken);
        var batteryCharge = await ReadNumericStateAsync(bindings.BatteryChargePowerEntityId, cancellationToken);
        var batteryDischarge = await ReadNumericStateAsync(bindings.BatteryDischargePowerEntityId, cancellationToken);

        return new EnergyState(
            pvProduction,
            gridImport,
            gridExport,
            batterySoc,
            batteryCharge,
            batteryDischarge);
    }

    private async Task<int> ReadNumericStateAsync(string? entityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            return 0;
        }

        var entity = await _stateReader.GetStateAsync(entityId, cancellationToken);
        if (entity is null)
        {
            return 0;
        }

        if (int.TryParse(entity.State, out var intValue))
        {
            return intValue;
        }

        if (double.TryParse(entity.State, out var doubleValue))
        {
            return (int)Math.Round(doubleValue);
        }

        return 0;
    }

    private async Task<PresenceMode> ResolvePresenceModeAsync(
        Application.Contracts.HomeAssistantEntityBindings bindings,
        CancellationToken cancellationToken)
    {
        if (bindings.PresenceEntityIds.Count == 0)
        {
            return PresenceMode.EveryoneHome;
        }

        var states = new List<string>();
        foreach (var entityId in bindings.PresenceEntityIds)
        {
            var entity = await _stateReader.GetStateAsync(entityId, cancellationToken);
            if (entity is not null)
            {
                states.Add(entity.State);
            }
        }

        if (states.Count == 0)
        {
            return PresenceMode.EveryoneHome;
        }

        var anyoneHome = states.Any(x => x.Equals("home", StringComparison.OrdinalIgnoreCase));
        return anyoneHome ? PresenceMode.EveryoneHome : PresenceMode.NobodyHome;
    }

    private static ClimateEntityState BuildClimateState(string entityId, Application.Contracts.HomeAssistantEntityDto? entity)
    {
        if (entity is null)
        {
            return new ClimateEntityState(entityId, null, new HashSet<ThermostatPresetMode>(), null, null);
        }

        var supportedPresets = new HashSet<ThermostatPresetMode>();
        if (entity.Attributes.TryGetValue("preset_modes", out var presetModesValue)
            && presetModesValue is IEnumerable<object?> presetModes)
        {
            foreach (var value in presetModes)
            {
                var mode = TryParsePreset(value?.ToString());
                if (mode.HasValue)
                {
                    supportedPresets.Add(mode.Value);
                }
            }
        }

        ThermostatPresetMode? currentPreset = null;
        if (entity.Attributes.TryGetValue("preset_mode", out var presetModeValue))
        {
            currentPreset = TryParsePreset(presetModeValue?.ToString());
        }

        return new ClimateEntityState(
            entityId,
            currentPreset,
            supportedPresets,
            null,
            null);
    }

    private static ThermostatPresetMode? TryParsePreset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.ToLowerInvariant() switch
        {
            "none" => ThermostatPresetMode.None,
            "away" => ThermostatPresetMode.Away,
            "comfort" => ThermostatPresetMode.Comfort,
            "eco" => ThermostatPresetMode.Eco,
            "home" => ThermostatPresetMode.Home,
            "sleep" => ThermostatPresetMode.Sleep,
            "activity" => ThermostatPresetMode.Activity,
            _ => null
        };
    }

    private static bool IsNightMode(DateTimeOffset utcNow)
    {
        var localHour = utcNow.ToLocalTime().Hour;
        return localHour >= 22 || localHour < 6;
    }

    private static EnergyMode ResolveEnergyMode(EnergyState energyState, PowerManagerSettings settings)
    {
        if (energyState.GridImportWatts > settings.MaxGridImportWatts)
        {
            return EnergyMode.ExpensivePower;
        }

        if (energyState.GridExportWatts > settings.SafetyMarginWatts * 3)
        {
            return EnergyMode.HighSurplusPower;
        }

        if (energyState.GridExportWatts > settings.SafetyMarginWatts)
        {
            return EnergyMode.SurplusPower;
        }

        if (energyState.GridExportWatts > 0)
        {
            return EnergyMode.FreePower;
        }

        return EnergyMode.NormalPower;
    }

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Error,
        Message = "Failed to dispatch HA command for {ClimateEntityId}")]
    private static partial void LogDispatchFailure(ILogger logger, string climateEntityId, Exception exception);
}
