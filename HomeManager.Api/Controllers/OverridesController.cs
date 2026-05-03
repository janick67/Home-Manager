using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/overrides")]
public sealed class OverridesController : ControllerBase
{
    private readonly IOverrideRepository _overrideRepository;

    public OverridesController(IOverrideRepository overrideRepository)
    {
        _overrideRepository = overrideRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetOverrides(CancellationToken cancellationToken)
    {
        return Ok(await _overrideRepository.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOverride([FromBody] ManualOverrideDefinition entry, CancellationToken cancellationToken)
    {
        var toCreate = entry with { Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id };
        await _overrideRepository.AddAsync(toCreate, cancellationToken);
        return CreatedAtAction(nameof(GetOverrides), new { id = toCreate.Id }, toCreate);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteOverride(Guid id, CancellationToken cancellationToken)
    {
        await _overrideRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
