using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class DecisionHistoryRepository : IDecisionHistoryRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public DecisionHistoryRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PowerManagerDecisionResult decisionResult, CancellationToken cancellationToken)
    {
        foreach (var roomDecision in decisionResult.RoomDecisions)
        {
            _dbContext.Decisions.Add(new DecisionEntity
            {
                Id = Guid.NewGuid(),
                TimestampUtc = decisionResult.EvaluatedAtUtc,
                ManagerName = "PowerManager",
                RoomId = roomDecision.RoomId,
                ClimateEntityId = roomDecision.ClimateEntityId,
                PreviousPreset = roomDecision.PreviousPreset?.ToString(),
                NewPreset = roomDecision.TargetPreset.ToString(),
                EnergyMode = decisionResult.EffectiveEnergyMode.ToString(),
                PresenceMode = decisionResult.EffectivePresenceMode.ToString(),
                AvailablePowerWatts = decisionResult.AvailablePowerWatts,
                GridImportWatts = 0,
                GridExportWatts = 0,
                BatterySocPercent = 0,
                BatteryDischargeWatts = 0,
                Reason = roomDecision.Reason.Message,
                CommandSent = roomDecision.CommandSent,
                CommandResult = null
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PowerManagerDecisionResult>> GetLatestAsync(int count, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Decisions
            .OrderByDescending(x => x.TimestampUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyCollection<PowerManagerDecisionResult>> GetHistoryAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Decisions
            .Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc <= toUtc)
            .OrderByDescending(x => x.TimestampUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomain).ToArray();
    }

    private static PowerManagerDecisionResult ToDomain(DecisionEntity entity)
    {
        var roomDecision = new RoomDecision(
            entity.RoomId,
            entity.ClimateEntityId,
            entity.ClimateEntityId,
            ParseNullableEnum<ThermostatPresetMode>(entity.PreviousPreset),
            ParseEnum(entity.NewPreset, ThermostatPresetMode.Home),
            new DecisionReason("History", entity.Reason),
            entity.CommandSent,
            null);

        return new PowerManagerDecisionResult(
            entity.TimestampUtc,
            ParseEnum(entity.EnergyMode, EnergyMode.NormalPower),
            ParseEnum(entity.PresenceMode, PresenceMode.EveryoneHome),
            entity.AvailablePowerWatts,
            [roomDecision]);
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : fallback;
    }

    private static TEnum? ParseNullableEnum<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : null;
    }
}
