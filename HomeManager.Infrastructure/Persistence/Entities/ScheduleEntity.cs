namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class ScheduleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string? PresenceMode { get; set; }
    public string? EnergyMode { get; set; }
    public DateTimeOffset? StartUtc { get; set; }
    public DateTimeOffset? EndUtc { get; set; }
    public string DaysOfWeekCsv { get; set; } = string.Empty;
    public TimeSpan? DailyStartTime { get; set; }
    public TimeSpan? DailyEndTime { get; set; }
    public int Priority { get; set; }
}
