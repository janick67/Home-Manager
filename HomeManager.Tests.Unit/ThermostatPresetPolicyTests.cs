using FluentAssertions;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Services;
using HomeManager.Tests.Unit.Helpers;

namespace HomeManager.Tests.Unit;

public sealed class ThermostatPresetPolicyTests
{
    private readonly ThermostatPresetPolicy _policy = new();

    [Fact]
    public void Should_MapPresetToHomeAssistantValue()
    {
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.None).Should().Be("none");
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.Away).Should().Be("away");
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.Comfort).Should().Be("comfort");
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.Eco).Should().Be("eco");
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.Home).Should().Be("home");
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.Sleep).Should().Be("sleep");
        _policy.ToHomeAssistantPresetValue(ThermostatPresetMode.Activity).Should().Be("activity");
    }

    [Fact]
    public void Should_FallbackWhenRequestedPresetUnsupported()
    {
        var room = TestDataFactory.CreateRoom(
            "Buffer",
            "climate.buffer",
            500,
            1,
            storagePreset: ThermostatPresetMode.Activity,
            defaultPreset: ThermostatPresetMode.Home,
            ecoPreset: ThermostatPresetMode.Eco);

        var state = TestDataFactory.CreateClimateState(
            room.ClimateEntityId,
            ThermostatPresetMode.Eco,
            supportedPresets: new HashSet<ThermostatPresetMode> { ThermostatPresetMode.Home, ThermostatPresetMode.Eco });

        var result = _policy.SelectSupportedPreset(ThermostatPresetMode.Activity, room, state);

        result.AppliedPreset.Should().Be(ThermostatPresetMode.Home);
        result.FallbackUsed.Should().BeTrue();
    }
}
