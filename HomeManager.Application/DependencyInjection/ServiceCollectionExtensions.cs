using HomeManager.Application.Services;
using HomeManager.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HomeManager.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ThermostatPresetPolicy>();
        services.AddScoped<IPowerManagerDecisionEngine, PowerManagerDecisionEngine>();
        services.AddScoped<IOverridePriorityResolver, OverridePriorityResolver>();
        services.AddScoped<IScheduleEvaluator, ScheduleEvaluator>();
        services.AddScoped<IRoomConfigurationValidator, RoomConfigurationValidator>();
        return services;
    }
}
