using HomeManager.Domain.Models;

namespace HomeManager.Application.Ports.Repositories;

public interface IRoomRepository
{
    Task<IReadOnlyCollection<RoomConfiguration>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoomConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(RoomConfiguration room, CancellationToken cancellationToken);
    Task UpdateAsync(RoomConfiguration room, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
