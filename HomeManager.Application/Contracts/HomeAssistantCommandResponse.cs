namespace HomeManager.Application.Contracts;

public sealed record HomeAssistantCommandResponse(
    bool Success,
    string Message,
    string? RawResponse);
