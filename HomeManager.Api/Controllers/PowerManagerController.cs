using HomeManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/managers/power")]
public sealed class PowerManagerController : ControllerBase
{
    private readonly PowerManagerOrchestrator _orchestrator;

    public PowerManagerController(PowerManagerOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromQuery] bool sendCommands = true, CancellationToken cancellationToken = default)
    {
        var result = await _orchestrator.EvaluateAsync(sendCommands, cancellationToken);
        return Ok(result);
    }
}
