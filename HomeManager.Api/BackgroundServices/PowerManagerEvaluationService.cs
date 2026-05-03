using HomeManager.Api.Services;
using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;

namespace HomeManager.Api.BackgroundServices;

public sealed partial class PowerManagerEvaluationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PowerManagerEvaluationService> _logger;

    public PowerManagerEvaluationService(IServiceProvider serviceProvider, ILogger<PowerManagerEvaluationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delaySeconds = 60;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<PowerManagerOrchestrator>();
                var settingsRepository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
                var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

                var settings = await settingsRepository.GetPowerManagerSettingsAsync(stoppingToken);
                delaySeconds = Math.Max(30, settings.EvaluationIntervalSeconds);

                var result = await orchestrator.EvaluateAsync(true, stoppingToken);
                await logRepository.AddAsync(
                    new LogEntryDto(
                        DateTimeOffset.UtcNow,
                        "Information",
                        $"Power manager evaluated {result.RoomDecisions.Count} rooms.",
                        null),
                    stoppingToken);
            }
            catch (Exception exception)
            {
                LogPowerManagerEvaluationFailed(_logger, exception);
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Warning,
        Message = "Power manager background evaluation failed.")]
    private static partial void LogPowerManagerEvaluationFailed(ILogger logger, Exception exception);
}
