namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class HaCommandEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public string ClimateEntityId { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ResultMessage { get; set; }
}
