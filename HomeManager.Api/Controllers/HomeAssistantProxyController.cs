using HomeManager.Api.Models;
using HomeManager.Application.Ports.HomeAssistant;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class HomeAssistantProxyController : ControllerBase
{
    private readonly IHomeAssistantStateReader _stateReader;
    private readonly IHomeAssistantCommandSender _commandSender;

    public HomeAssistantProxyController(
        IHomeAssistantStateReader stateReader,
        IHomeAssistantCommandSender commandSender)
    {
        _stateReader = stateReader;
        _commandSender = commandSender;
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates(CancellationToken cancellationToken)
    {
        var result = await _stateReader.GetStatesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("states/{entityId}")]
    public async Task<IActionResult> GetState(string entityId, CancellationToken cancellationToken)
    {
        var result = await _stateReader.GetStateAsync(entityId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("services/{domain}/{service}")]
    public async Task<IActionResult> CallService(
        string domain,
        string service,
        [FromBody] ServiceCallRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _commandSender.CallServiceAsync(domain, service, request.Data, cancellationToken);
        return Ok(response);
    }
}
