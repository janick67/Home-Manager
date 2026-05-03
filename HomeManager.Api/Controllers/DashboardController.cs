using HomeManager.Api.Models;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IHaEntityRepository _haEntityRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IOverrideRepository _overrideRepository;
    private readonly IDecisionHistoryRepository _decisionHistoryRepository;

    public DashboardController(
        ISettingsRepository settingsRepository,
        IHaEntityRepository haEntityRepository,
        IScheduleRepository scheduleRepository,
        IOverrideRepository overrideRepository,
        IDecisionHistoryRepository decisionHistoryRepository)
    {
        _settingsRepository = settingsRepository;
        _haEntityRepository = haEntityRepository;
        _scheduleRepository = scheduleRepository;
        _overrideRepository = overrideRepository;
        _decisionHistoryRepository = decisionHistoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetPowerManagerSettingsAsync(cancellationToken);
        var bindings = await _settingsRepository.GetEntityBindingsAsync(cancellationToken);
        var entities = await _haEntityRepository.GetAllAsync(cancellationToken);
        var schedules = await _scheduleRepository.GetAllAsync(cancellationToken);
        var overrides = await _overrideRepository.GetAllAsync(cancellationToken);
        var latestDecisions = await _decisionHistoryRepository.GetLatestAsync(10, cancellationToken);

        var pv = ReadInt(entities, bindings.PvProductionEntityId);
        var gridImport = ReadInt(entities, bindings.GridImportEntityId);
        var gridExport = ReadInt(entities, bindings.GridExportEntityId);
        var batterySoc = ReadInt(entities, bindings.BatterySocEntityId);
        var batteryCharge = ReadInt(entities, bindings.BatteryChargePowerEntityId);
        var batteryDischarge = ReadInt(entities, bindings.BatteryDischargePowerEntityId);
        var availablePower = gridExport - settings.SafetyMarginWatts;

        var dashboard = new DashboardDto(
            HomeAssistantConnected: entities.Count > 0,
            EnergyMode: availablePower > 0 ? EnergyMode.SurplusPower : EnergyMode.NormalPower,
            PresenceMode: PresenceMode.EveryoneHome,
            PvProductionWatts: pv,
            GridImportWatts: gridImport,
            GridExportWatts: gridExport,
            BatterySocPercent: batterySoc,
            BatteryChargeWatts: batteryCharge,
            BatteryDischargeWatts: batteryDischarge,
            AvailablePowerWatts: availablePower,
            ActiveSchedules: schedules.Count,
            ActiveOverrides: overrides.Count,
            LatestDecisions: latestDecisions);

        return Ok(dashboard);
    }

    private static int ReadInt(
        IReadOnlyCollection<Application.Contracts.HomeAssistantEntityDto> entities,
        string? entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            return 0;
        }

        var entity = entities.FirstOrDefault(x => x.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
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
}
