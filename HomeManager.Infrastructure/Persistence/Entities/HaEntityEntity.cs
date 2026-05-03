namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class HaEntityEntity
{
    public string EntityId { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? FriendlyName { get; set; }
    public string AttributesJson { get; set; } = "{}";
    public DateTimeOffset LastSeenAtUtc { get; set; }
}
