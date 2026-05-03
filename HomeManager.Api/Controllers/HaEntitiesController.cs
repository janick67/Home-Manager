using HomeManager.Application.Ports.HomeAssistant;
using HomeManager.Application.Ports.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/ha")]
public sealed class HaEntitiesController : ControllerBase
{
    private readonly IHaEntityRepository _haEntityRepository;
    private readonly IHomeAssistantStateReader _stateReader;

    public HaEntitiesController(
        IHaEntityRepository haEntityRepository,
        IHomeAssistantStateReader stateReader)
    {
        _haEntityRepository = haEntityRepository;
        _stateReader = stateReader;
    }

    [HttpGet("entities")]
    public async Task<IActionResult> GetEntities(CancellationToken cancellationToken)
    {
        return Ok(await _haEntityRepository.GetAllAsync(cancellationToken));
    }

    [HttpPost("refresh-entities")]
    public async Task<IActionResult> RefreshEntities(CancellationToken cancellationToken)
    {
        var entities = await _stateReader.GetStatesAsync(cancellationToken);
        await _haEntityRepository.UpsertAsync(entities, cancellationToken);
        return Ok(new { refreshed = entities.Count });
    }
}
