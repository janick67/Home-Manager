namespace HomeManager.Domain.Models;

public sealed record HaCommandRequest(
    string Domain,
    string Service,
    string EntityId,
    IReadOnlyDictionary<string, object?> Data);
