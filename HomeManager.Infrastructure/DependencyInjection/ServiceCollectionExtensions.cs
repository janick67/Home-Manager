using System.Net.Http.Headers;
using HomeManager.Application.Abstractions;
using HomeManager.Application.Ports.HomeAssistant;
using HomeManager.Application.Ports.Repositories;
using HomeManager.Infrastructure.HomeAssistant;
using HomeManager.Infrastructure.Persistence;
using HomeManager.Infrastructure.Persistence.Repositories;
using HomeManager.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HomeManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MariaDb")
            ?? throw new InvalidOperationException("ConnectionStrings:MariaDb is missing.");

        services.AddDbContext<HomeManagerDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddOptions<HomeAssistantOptions>()
            .Bind(configuration.GetSection("HomeAssistant"))
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute),
                "HomeAssistant:BaseUrl must be a valid absolute URL.")
            .ValidateOnStart();

        services.AddHttpClient<IHomeAssistantClient, HomeAssistantClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<HomeAssistantOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + '/');
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.LongLivedAccessToken);
        });

        services.AddScoped<IHomeAssistantStateReader>(serviceProvider => serviceProvider.GetRequiredService<IHomeAssistantClient>());
        services.AddScoped<IHomeAssistantCommandSender>(serviceProvider => serviceProvider.GetRequiredService<IHomeAssistantClient>());

        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IOverrideRepository, OverrideRepository>();
        services.AddScoped<IDecisionHistoryRepository, DecisionHistoryRepository>();
        services.AddScoped<IHaCommandHistoryRepository, HaCommandHistoryRepository>();
        services.AddScoped<IHaEntityRepository, HaEntityRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<IManagerStateRepository, ManagerStateRepository>();

        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
