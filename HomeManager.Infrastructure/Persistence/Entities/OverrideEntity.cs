namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class OverrideEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string? PresenceMode { get; set; }
    public string? EnergyMode { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public int Priority { get; set; }
}
