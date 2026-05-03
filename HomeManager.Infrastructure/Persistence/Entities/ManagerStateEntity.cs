namespace HomeManager.Infrastructure.Persistence.Entities;

public sealed class ManagerStateEntity
{
    public string ManagerName { get; set; } = string.Empty;
    public DateTimeOffset? LastHeartbeatAtUtc { get; set; }
    public DateTimeOffset? LastEvaluationAtUtc { get; set; }
}
