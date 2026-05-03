using HomeManager.Application.Ports.Repositories;
using HomeManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/schedules")]
public sealed class SchedulesController : ControllerBase
{
    private readonly IScheduleRepository _scheduleRepository;

    public SchedulesController(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules(CancellationToken cancellationToken)
    {
        return Ok(await _scheduleRepository.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] ScheduleDefinition schedule, CancellationToken cancellationToken)
    {
        var scheduleToCreate = schedule with { Id = schedule.Id == Guid.Empty ? Guid.NewGuid() : schedule.Id };
        await _scheduleRepository.AddAsync(scheduleToCreate, cancellationToken);
        return CreatedAtAction(nameof(GetSchedules), new { id = scheduleToCreate.Id }, scheduleToCreate);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] ScheduleDefinition schedule, CancellationToken cancellationToken)
    {
        await _scheduleRepository.UpdateAsync(schedule with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id, CancellationToken cancellationToken)
    {
        await _scheduleRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
