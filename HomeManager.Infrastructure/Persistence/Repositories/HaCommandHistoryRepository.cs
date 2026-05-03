using System.Text.Json;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Models;
using HomeManager.Infrastructure.Persistence.Entities;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class HaCommandHistoryRepository : IHaCommandHistoryRepository
{
    private readonly HomeManagerDbContext _dbContext;

    public HaCommandHistoryRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RoomDecision roomDecision, string commandResult, CancellationToken cancellationToken)
    {
        var payloadJson = roomDecision.CommandRequest is null
            ? "{}"
            : JsonSerializer.Serialize(roomDecision.CommandRequest.Data);

        _dbContext.HaCommands.Add(new HaCommandEntity
        {
            Id = Guid.NewGuid(),
            TimestampUtc = DateTimeOffset.UtcNow,
            ClimateEntityId = roomDecision.ClimateEntityId,
            Domain = roomDecision.CommandRequest?.Domain ?? string.Empty,
            Service = roomDecision.CommandRequest?.Service ?? string.Empty,
            PayloadJson = payloadJson,
            Success = commandResult.StartsWith("OK", StringComparison.OrdinalIgnoreCase),
            ResultMessage = commandResult
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
