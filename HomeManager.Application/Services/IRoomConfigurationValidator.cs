using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public interface IRoomConfigurationValidator
{
    IReadOnlyCollection<string> Validate(RoomConfiguration configuration);
}
