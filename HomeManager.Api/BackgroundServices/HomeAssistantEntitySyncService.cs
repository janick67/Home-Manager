using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.HomeAssistant;
using HomeManager.Application.Ports.Repositories;

namespace HomeManager.Api.BackgroundServices;

public sealed partial class HomeAssistantEntitySyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HomeAssistantEntitySyncService> _logger;

    public HomeAssistantEntitySyncService(IServiceProvider serviceProvider, ILogger<HomeAssistantEntitySyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stateReader = scope.ServiceProvider.GetRequiredService<IHomeAssistantStateReader>();
                var repository = scope.ServiceProvider.GetRequiredService<IHaEntityRepository>();
                var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

                var entities = await stateReader.GetStatesAsync(stoppingToken);
                await repository.UpsertAsync(entities, stoppingToken);
                await logRepository.AddAsync(
                    new LogEntryDto(DateTimeOffset.UtcNow, "Information", $"Synced {entities.Count} HA entities.", null),
                    stoppingToken);
            }
            catch (Exception exception)
            {
                LogEntitySyncFailed(_logger, exception);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Entity sync failed.")]
    private static partial void LogEntitySyncFailed(ILogger logger, Exception exception);
}
