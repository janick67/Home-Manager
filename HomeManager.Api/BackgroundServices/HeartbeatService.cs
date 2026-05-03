using System.Globalization;
using HomeManager.Application.Abstractions;
using HomeManager.Application.Ports.HomeAssistant;
using HomeManager.Application.Ports.Repositories;

namespace HomeManager.Api.BackgroundServices;

public sealed partial class HeartbeatService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IClock _clock;
    private readonly ILogger<HeartbeatService> _logger;

    public HeartbeatService(IServiceProvider serviceProvider, IClock clock, ILogger<HeartbeatService> logger)
    {
        _serviceProvider = serviceProvider;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var commandSender = scope.ServiceProvider.GetRequiredService<IHomeAssistantCommandSender>();
                var managerStateRepository = scope.ServiceProvider.GetRequiredService<IManagerStateRepository>();
                var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

                var settings = await settingsRepository.GetPowerManagerSettingsAsync(stoppingToken);
                var heartbeatPayload = new Dictionary<string, object?>
                {
                    ["entity_id"] = "input_datetime.heating_manager_last_seen",
                    ["datetime"] = _clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                };

                await commandSender.CallServiceAsync(
                    "input_datetime",
                    "set_datetime",
                    heartbeatPayload,
                    stoppingToken);

                await managerStateRepository.SetLastHeartbeatAsync("PowerManager", _clock.UtcNow, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(30, settings.HeartbeatIntervalSeconds)), stoppingToken);
            }
            catch (Exception exception)
            {
                LogHeartbeatFailed(_logger, exception);
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Heartbeat failed.")]
    private static partial void LogHeartbeatFailed(ILogger logger, Exception exception);
}
