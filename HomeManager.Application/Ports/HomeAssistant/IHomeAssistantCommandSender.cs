using HomeManager.Application.Contracts;

namespace HomeManager.Application.Ports.HomeAssistant;

public interface IHomeAssistantCommandSender
{
    Task<HomeAssistantCommandResponse> CallServiceAsync(
        string domain,
        string service,
        IReadOnlyDictionary<string, object?> serviceData,
        CancellationToken cancellationToken);
}
