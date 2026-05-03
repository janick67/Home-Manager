namespace HomeManager.Api.Models;

public sealed record ServiceCallRequest(
    IReadOnlyDictionary<string, object?> Data);
