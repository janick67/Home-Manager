using System.Text.Json.Serialization;
using HomeManager.Api.BackgroundServices;
using HomeManager.Api.Middleware;
using HomeManager.Api.Services;
using HomeManager.Application.DependencyInjection;
using HomeManager.Infrastructure.DependencyInjection;
using HomeManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<PowerManagerOrchestrator>();

builder.Services.AddHostedService<HomeAssistantEntitySyncService>();
builder.Services.AddHostedService<HeartbeatService>();
builder.Services.AddHostedService<ScheduleEvaluatorService>();
builder.Services.AddHostedService<PowerManagerEvaluationService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<HomeManagerDbContext>("mariadb");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HomeManagerDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
