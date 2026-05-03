using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Models;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public SettingsRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HomeAssistantConnectionSettings> GetHomeAssistantConnectionAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureSettingsEntityAsync(cancellationToken);
        return new HomeAssistantConnectionSettings(entity.HomeAssistantBaseUrl, entity.HomeAssistantToken);
    }

    public async Task SaveHomeAssistantConnectionAsync(HomeAssistantConnectionSettings settings, CancellationToken cancellationToken)
    {
        var entity = await EnsureSettingsEntityAsync(cancellationToken);
        entity.HomeAssistantBaseUrl = settings.BaseUrl;
        entity.HomeAssistantToken = settings.LongLivedAccessToken;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PowerManagerSettings> GetPowerManagerSettingsAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureSettingsEntityAsync(cancellationToken);
        return new PowerManagerSettings(
            entity.SafetyMarginWatts,
            entity.MaxGridImportWatts,
            entity.MaxBatteryDischargeWatts,
            entity.MinBatterySocForStoragePercent,
            entity.EvaluationIntervalSeconds,
            entity.CommandCooldownSeconds,
            entity.HeartbeatIntervalSeconds);
    }

    public async Task SavePowerManagerSettingsAsync(PowerManagerSettings settings, CancellationToken cancellationToken)
    {
        var entity = await EnsureSettingsEntityAsync(cancellationToken);
        entity.SafetyMarginWatts = settings.SafetyMarginWatts;
        entity.MaxGridImportWatts = settings.MaxGridImportWatts;
        entity.MaxBatteryDischargeWatts = settings.MaxBatteryDischargeWatts;
        entity.MinBatterySocForStoragePercent = settings.MinBatterySocForStoragePercent;
        entity.EvaluationIntervalSeconds = settings.EvaluationIntervalSeconds;
        entity.CommandCooldownSeconds = settings.CommandCooldownSeconds;
        entity.HeartbeatIntervalSeconds = settings.HeartbeatIntervalSeconds;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<HomeAssistantEntityBindings> GetEntityBindingsAsync(CancellationToken cancellationToken)
    {
        var entity = await EnsureSettingsEntityAsync(cancellationToken);
        return new HomeAssistantEntityBindings(
            entity.PvProductionEntityId,
            entity.GridImportEntityId,
            entity.GridExportEntityId,
            entity.BatterySocEntityId,
            entity.BatteryChargePowerEntityId,
            entity.BatteryDischargePowerEntityId,
            entity.PresenceEntityIdsCsv
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    public async Task SaveEntityBindingsAsync(HomeAssistantEntityBindings bindings, CancellationToken cancellationToken)
    {
        var entity = await EnsureSettingsEntityAsync(cancellationToken);
        entity.PvProductionEntityId = bindings.PvProductionEntityId;
        entity.GridImportEntityId = bindings.GridImportEntityId;
        entity.GridExportEntityId = bindings.GridExportEntityId;
        entity.BatterySocEntityId = bindings.BatterySocEntityId;
        entity.BatteryChargePowerEntityId = bindings.BatteryChargePowerEntityId;
        entity.BatteryDischargePowerEntityId = bindings.BatteryDischargePowerEntityId;
        entity.PresenceEntityIdsCsv = string.Join(',', bindings.PresenceEntityIds);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<SettingEntity> EnsureSettingsEntityAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Settings.FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var settings = new SettingEntity
        {
            Id = 1
        };

        _dbContext.Settings.Add(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
