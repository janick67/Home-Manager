using HomeManager.Api.Models;
using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsController(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var connection = await _settingsRepository.GetHomeAssistantConnectionAsync(cancellationToken);
        var power = await _settingsRepository.GetPowerManagerSettingsAsync(cancellationToken);
        var bindings = await _settingsRepository.GetEntityBindingsAsync(cancellationToken);

        return Ok(new SettingsDto(
            connection.BaseUrl,
            connection.LongLivedAccessToken,
            power.SafetyMarginWatts,
            power.MaxGridImportWatts,
            power.MaxBatteryDischargeWatts,
            power.MinBatterySocForStoragePercent,
            power.EvaluationIntervalSeconds,
            power.CommandCooldownSeconds,
            power.HeartbeatIntervalSeconds,
            bindings.PvProductionEntityId,
            bindings.GridImportEntityId,
            bindings.GridExportEntityId,
            bindings.BatterySocEntityId,
            bindings.BatteryChargePowerEntityId,
            bindings.BatteryDischargePowerEntityId,
            bindings.PresenceEntityIds));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto request, CancellationToken cancellationToken)
    {
        await _settingsRepository.SaveHomeAssistantConnectionAsync(
            new HomeAssistantConnectionSettings(request.HomeAssistantBaseUrl, request.HomeAssistantToken),
            cancellationToken);

        await _settingsRepository.SavePowerManagerSettingsAsync(
            new PowerManagerSettings(
                request.SafetyMarginWatts,
                request.MaxGridImportWatts,
                request.MaxBatteryDischargeWatts,
                request.MinBatterySocForStoragePercent,
                request.EvaluationIntervalSeconds,
                request.CommandCooldownSeconds,
                request.HeartbeatIntervalSeconds),
            cancellationToken);

        await _settingsRepository.SaveEntityBindingsAsync(
            new HomeAssistantEntityBindings(
                request.PvProductionEntityId,
                request.GridImportEntityId,
                request.GridExportEntityId,
                request.BatterySocEntityId,
                request.BatteryChargePowerEntityId,
                request.BatteryDischargePowerEntityId,
                request.PresenceEntityIds),
            cancellationToken);

        return NoContent();
    }
}
