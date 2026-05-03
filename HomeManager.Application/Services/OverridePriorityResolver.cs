using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public sealed class OverridePriorityResolver : IOverridePriorityResolver
{
    public PresenceMode? ResolvePresenceOverride(IReadOnlyCollection<ManualOverrideDefinition> activeOverrides)
    {
        var candidates = activeOverrides
            .Where(x => x.TargetType == ScheduleTargetType.Presence && x.PresenceMode.HasValue)
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Any(x => x.PresenceMode == PresenceMode.Vacation))
        {
            return PresenceMode.Vacation;
        }

        return candidates
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.StartsAtUtc)
            .Select(x => x.PresenceMode)
            .FirstOrDefault();
    }

    public EnergyMode? ResolveEnergyOverride(IReadOnlyCollection<ManualOverrideDefinition> activeOverrides)
    {
        return activeOverrides
            .Where(x => x.TargetType == ScheduleTargetType.Energy && x.EnergyMode.HasValue)
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.StartsAtUtc)
            .Select(x => x.EnergyMode)
            .FirstOrDefault();
    }
}
