using HomeManager.Application.Contracts;
using HomeManager.Domain.Models;

namespace HomeManager.Application.Ports.Repositories;

public interface ISettingsRepository
{
    Task<HomeAssistantConnectionSettings> GetHomeAssistantConnectionAsync(CancellationToken cancellationToken);
    Task SaveHomeAssistantConnectionAsync(HomeAssistantConnectionSettings settings, CancellationToken cancellationToken);
    Task<PowerManagerSettings> GetPowerManagerSettingsAsync(CancellationToken cancellationToken);
    Task SavePowerManagerSettingsAsync(PowerManagerSettings settings, CancellationToken cancellationToken);
    Task<HomeAssistantEntityBindings> GetEntityBindingsAsync(CancellationToken cancellationToken);
    Task SaveEntityBindingsAsync(HomeAssistantEntityBindings bindings, CancellationToken cancellationToken);
}
