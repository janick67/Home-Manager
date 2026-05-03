using HomeManager.Application.Contracts;
using HomeManager.Application.Ports.Repositories;

namespace HomeManager.Api.Middleware;

public sealed partial class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILogRepository logRepository)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            LogUnhandledApiError(_logger, exception);
            await logRepository.AddAsync(
                new LogEntryDto(
                    DateTimeOffset.UtcNow,
                    "Error",
                    "Unhandled API exception",
                    exception.ToString()),
                context.RequestAborted);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(
                new
                {
                    error = "InternalServerError",
                    message = "Unexpected server error."
                },
                context.RequestAborted);
        }
    }

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Unhandled API error.")]
    private static partial void LogUnhandledApiError(ILogger logger, Exception exception);
}
