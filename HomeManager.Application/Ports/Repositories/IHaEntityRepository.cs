using HomeManager.Application.Contracts;

namespace HomeManager.Application.Ports.Repositories;

public interface IHaEntityRepository
{
    Task<IReadOnlyCollection<HomeAssistantEntityDto>> GetAllAsync(CancellationToken cancellationToken);
    Task UpsertAsync(IReadOnlyCollection<HomeAssistantEntityDto> entities, CancellationToken cancellationToken);
}
