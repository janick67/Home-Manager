using System.Net;
using System.Text;
using System.Text.Json;
using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.HomeAssistant;
using Microsoft.Extensions.Logging;

namespace HomeManager.Infrastructure.HomeAssistant;

public sealed partial class HomeAssistantClient : IHomeAssistantClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ILogger<HomeAssistantClient> _logger;

    public HomeAssistantClient(HttpClient httpClient, ILogger<HomeAssistantClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<HomeAssistantEntityDto>> GetStatesAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("api/states", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var entities = new List<HomeAssistantEntityDto>();
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return entities;
        }

        foreach (var element in document.RootElement.EnumerateArray())
        {
            entities.Add(ParseEntity(element));
        }

        return entities;
    }

    public async Task<HomeAssistantEntityDto?> GetStateAsync(string entityId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"api/states/{Uri.EscapeDataString(entityId)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return ParseEntity(document.RootElement);
    }

    public async Task<HomeAssistantCommandResponse> CallServiceAsync(
        string domain,
        string service,
        IReadOnlyDictionary<string, object?> serviceData,
        CancellationToken cancellationToken)
    {
        var jsonPayload = JsonSerializer.Serialize(serviceData, JsonOptions);
        using var payload = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(
            $"api/services/{Uri.EscapeDataString(domain)}/{Uri.EscapeDataString(service)}",
            payload,
            cancellationToken);

        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            LogServiceCallFailed(_logger, domain, service, response.StatusCode, rawResponse);

            return new HomeAssistantCommandResponse(false, $"HTTP {(int)response.StatusCode}", rawResponse);
        }

        return new HomeAssistantCommandResponse(true, "OK", rawResponse);
    }

    private static HomeAssistantEntityDto ParseEntity(JsonElement element)
    {
        var entityId = element.TryGetProperty("entity_id", out var entityIdProperty)
            ? entityIdProperty.GetString() ?? string.Empty
            : string.Empty;

        var state = element.TryGetProperty("state", out var stateProperty)
            ? stateProperty.GetString() ?? string.Empty
            : string.Empty;

        string? friendlyName = null;
        var attributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (element.TryGetProperty("attributes", out var attributesProperty))
        {
            attributes = ReadAttributes(attributesProperty);
            if (attributes.TryGetValue("friendly_name", out var value))
            {
                friendlyName = value?.ToString();
            }
        }

        var split = entityId.Split('.', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var domain = split.Length > 0 ? split[0] : string.Empty;

        return new HomeAssistantEntityDto(entityId, state, friendlyName, domain, attributes);
    }

    private static Dictionary<string, object?> ReadAttributes(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        var dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJson(property.Value);
        }

        return dictionary;
    }

    private static object? ConvertJson(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var intValue)
                ? intValue
                : element.TryGetDouble(out var doubleValue)
                    ? doubleValue
                    : element.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJson).ToArray(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ConvertJson(p.Value), StringComparer.OrdinalIgnoreCase),
            _ => element.GetRawText()
        };
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Home Assistant service call failed for {Domain}.{Service}. Status: {StatusCode}. Body: {Body}")]
    private static partial void LogServiceCallFailed(
        ILogger logger,
        string domain,
        string service,
        HttpStatusCode statusCode,
        string body);
}
