using System.Text.Json;
using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeManager.Infrastructure.Persistence.Repositories;

public sealed class HaEntityRepository : IHaEntityRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HomeManagerDbContext _dbContext;

    public HaEntityRepository(HomeManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<HomeAssistantEntityDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.HaEntities
            .OrderBy(x => x.EntityId)
            .ToListAsync(cancellationToken);

        return entities.Select(entity =>
        {
            var attributes = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.AttributesJson, JsonOptions)
                ?? new Dictionary<string, object?>();

            return new HomeAssistantEntityDto(
                entity.EntityId,
                entity.State,
                entity.FriendlyName,
                entity.Domain,
                attributes);
        }).ToArray();
    }

    public async Task UpsertAsync(IReadOnlyCollection<HomeAssistantEntityDto> entities, CancellationToken cancellationToken)
    {
        foreach (var incoming in entities)
        {
            var existing = await _dbContext.HaEntities.FindAsync([incoming.EntityId], cancellationToken);
            if (existing is null)
            {
                _dbContext.HaEntities.Add(new HaEntityEntity
                {
                    EntityId = incoming.EntityId,
                    Domain = incoming.Domain,
                    State = incoming.State,
                    FriendlyName = incoming.FriendlyName,
                    AttributesJson = JsonSerializer.Serialize(incoming.Attributes, JsonOptions),
                    LastSeenAtUtc = DateTimeOffset.UtcNow
                });

                continue;
            }

            existing.Domain = incoming.Domain;
            existing.State = incoming.State;
            existing.FriendlyName = incoming.FriendlyName;
            existing.AttributesJson = JsonSerializer.Serialize(incoming.Attributes, JsonOptions);
            existing.LastSeenAtUtc = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
