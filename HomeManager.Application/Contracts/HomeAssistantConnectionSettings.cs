namespace HomeManager.Application.Contracts;

public sealed record HomeAssistantConnectionSettings(
    string BaseUrl,
    string LongLivedAccessToken);
