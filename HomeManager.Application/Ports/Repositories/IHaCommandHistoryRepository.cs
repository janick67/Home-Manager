using HomeManager.Domain.Models;

namespace HomeManager.Application.Ports.Repositories;

public interface IHaCommandHistoryRepository
{
    Task AddAsync(RoomDecision roomDecision, string commandResult, CancellationToken cancellationToken);
}
