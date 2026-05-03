using FluentAssertions;
using HomeManager.Application.Services;
using HomeManager.Domain.Enums;
using HomeManager.Tests.Unit.Helpers;

namespace HomeManager.Tests.Unit;

public sealed class RoomConfigurationValidatorTests
{
    private readonly RoomConfigurationValidator _validator = new();

    [Fact]
    public void Should_ReturnErrors_WhenConfigurationIsInvalid()
    {
        var room = TestDataFactory.CreateRoom(
            name: string.Empty,
            climateEntityId: "sensor.invalid",
            powerWatts: 0,
            priority: -1,
            canStoreHeat: true,
            allowPvBoost: true,
            storagePreset: null,
            commandCooldownSeconds: -1);

        var errors = _validator.Validate(room);

        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Should_ReturnNoErrors_WhenConfigurationIsValid()
    {
        var room = TestDataFactory.CreateRoom(
            "Salon",
            "climate.salon",
            600,
            1,
            roomType: RoomType.LivingRoom,
            storagePreset: ThermostatPresetMode.Comfort);

        var errors = _validator.Validate(room);

        errors.Should().BeEmpty();
    }
}
