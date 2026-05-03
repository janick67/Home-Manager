using HomeManager.Application.Ports.Repositories;
using HomeManager.Application.Services;
using HomeManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeManager.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomConfigurationValidator _validator;

    public RoomsController(IRoomRepository roomRepository, IRoomConfigurationValidator validator)
    {
        _roomRepository = roomRepository;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms(CancellationToken cancellationToken)
    {
        return Ok(await _roomRepository.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] RoomConfiguration room, CancellationToken cancellationToken)
    {
        var roomToCreate = room with { Id = room.Id == Guid.Empty ? Guid.NewGuid() : room.Id };
        var errors = _validator.Validate(roomToCreate);
        if (errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        await _roomRepository.AddAsync(roomToCreate, cancellationToken);
        return CreatedAtAction(nameof(GetRooms), new { id = roomToCreate.Id }, roomToCreate);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] RoomConfiguration room, CancellationToken cancellationToken)
    {
        var roomToUpdate = room with { Id = id };
        var errors = _validator.Validate(roomToUpdate);
        if (errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        await _roomRepository.UpdateAsync(roomToUpdate, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        await _roomRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
