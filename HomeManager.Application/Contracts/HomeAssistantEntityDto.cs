namespace HomeManager.Application.Contracts;

public sealed record HomeAssistantEntityDto(
    string EntityId,
    string State,
    string? FriendlyName,
    string Domain,
    IReadOnlyDictionary<string, object?> Attributes);
