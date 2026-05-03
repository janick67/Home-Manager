using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Application.Services;

namespace HomeManager.Api.BackgroundServices;

public sealed partial class ScheduleEvaluatorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduleEvaluatorService> _logger;

    public ScheduleEvaluatorService(IServiceProvider serviceProvider, ILogger<ScheduleEvaluatorService> logger)
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
                var scheduleRepository = scope.ServiceProvider.GetRequiredService<IScheduleRepository>();
                var overrideRepository = scope.ServiceProvider.GetRequiredService<IOverrideRepository>();
                var evaluator = scope.ServiceProvider.GetRequiredService<IScheduleEvaluator>();
                var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

                var schedules = await scheduleRepository.GetAllAsync(stoppingToken);
                var overrides = await overrideRepository.GetAllAsync(stoppingToken);
                var state = evaluator.Evaluate(schedules, overrides, false);

                await logRepository.AddAsync(
                    new LogEntryDto(
                        DateTimeOffset.UtcNow,
                        "Debug",
                        $"Schedule state: scheduledPresence={state.ScheduledPresenceMode}, manualPresence={state.ManualPresenceOverride}",
                        null),
                    stoppingToken);
            }
            catch (Exception exception)
            {
                LogScheduleEvaluatorCycleFailed(_logger, exception);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Schedule evaluator background cycle failed.")]
    private static partial void LogScheduleEvaluatorCycleFailed(ILogger logger, Exception exception);
}
