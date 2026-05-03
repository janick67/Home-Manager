using HomeManager.Application.Ports.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/decisions")]
public sealed class DecisionsController : ControllerBase
{
    private readonly IDecisionHistoryRepository _repository;

    public DecisionsController(IDecisionHistoryRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] int count = 20, CancellationToken cancellationToken = default)
    {
        var boundedCount = Math.Clamp(count, 1, 500);
        return Ok(await _repository.GetLatestAsync(boundedCount, cancellationToken));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateTimeOffset fromUtc,
        [FromQuery] DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _repository.GetHistoryAsync(fromUtc, toUtc, cancellationToken));
    }
}
