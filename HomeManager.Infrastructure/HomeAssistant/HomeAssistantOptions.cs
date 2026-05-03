using System.ComponentModel.DataAnnotations;

namespace HomeManager.Infrastructure.HomeAssistant;

public sealed class HomeAssistantOptions
{
    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string LongLivedAccessToken { get; init; } = string.Empty;
}
