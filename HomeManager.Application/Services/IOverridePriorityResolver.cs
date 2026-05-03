using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public interface IOverridePriorityResolver
{
    PresenceMode? ResolvePresenceOverride(IReadOnlyCollection<ManualOverrideDefinition> activeOverrides);
    EnergyMode? ResolveEnergyOverride(IReadOnlyCollection<ManualOverrideDefinition> activeOverrides);
}
