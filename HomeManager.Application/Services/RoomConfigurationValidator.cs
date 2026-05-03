using HomeManager.Domain.Models;

namespace HomeManager.Application.Services;

public sealed class RoomConfigurationValidator : IRoomConfigurationValidator
{
    public IReadOnlyCollection<string> Validate(RoomConfiguration configuration)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            errors.Add("Room name is required.");
        }

        if (string.IsNullOrWhiteSpace(configuration.ClimateEntityId))
        {
            errors.Add("Climate entity id is required.");
        }
        else if (!configuration.ClimateEntityId.StartsWith("climate.", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Climate entity id must start with 'climate.'.");
        }

        if (configuration.PowerWatts <= 0)
        {
            errors.Add("PowerWatts must be greater than 0.");
        }

        if (configuration.Priority < 0)
        {
            errors.Add("Priority cannot be negative.");
        }

        if (configuration.MinOnTimeSeconds < 0 || configuration.MinOffTimeSeconds < 0)
        {
            errors.Add("MinOnTimeSeconds and MinOffTimeSeconds cannot be negative.");
        }

        if (configuration.CommandCooldownSeconds < 0)
        {
            errors.Add("CommandCooldownSeconds cannot be negative.");
        }

        if (configuration.CanStoreHeat && configuration.AllowPvBoost && !configuration.Presets.StoragePreset.HasValue)
        {
            errors.Add("StoragePreset is required when room allows PV boost and can store heat.");
        }

        return errors;
    }
}
