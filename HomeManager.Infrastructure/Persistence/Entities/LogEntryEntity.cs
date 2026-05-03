namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class LogEntryEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public string Level { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
