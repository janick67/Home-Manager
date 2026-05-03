using HomeManager.Application.Ports.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogRepository _logRepository;

    public LogsController(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int count = 100, CancellationToken cancellationToken = default)
    {
        var boundedCount = Math.Clamp(count, 1, 1000);
        return Ok(await _logRepository.GetRecentAsync(boundedCount, cancellationToken));
    }
}
