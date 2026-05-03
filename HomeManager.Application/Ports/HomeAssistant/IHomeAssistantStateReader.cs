using HomeManager.Application.Contracts;

namespace HomeManager.Application.Ports.HomeAssistant;

public interface IHomeAssistantStateReader
{
    Task<IReadOnlyCollection<HomeAssistantEntityDto>> GetStatesAsync(CancellationToken cancellationToken);
    Task<HomeAssistantEntityDto?> GetStateAsync(string entityId, CancellationToken cancellationToken);
}
