using FluentAssertions;
using HomeManager.Application.Services;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Domain.Services;
using HomeManager.Tests.Unit.Helpers;

namespace HomeManager.Tests.Unit;

public sealed class PowerManagerDecisionEngineTests
{
    private readonly TestClock _clock;
    private readonly PowerManagerDecisionEngine _engine;

    public PowerManagerDecisionEngineTests()
    {
        _clock = new TestClock { UtcNow = new DateTimeOffset(2026, 5, 2, 21, 0, 0, TimeSpan.Zero) };
        _engine = new PowerManagerDecisionEngine(_clock, new ThermostatPresetPolicy());
    }

    [Fact]
    public void Should_EnableStorageOnlyWithinAvailableBudget()
    {
        var roomA = TestDataFactory.CreateRoom("R1", "climate.r1", 400, 1);
        var roomB = TestDataFactory.CreateRoom("R2", "climate.r2", 1200, 2);
        var roomC = TestDataFactory.CreateRoom("R3", "climate.r3", 600, 3);

        var input = BuildInput(
            rooms: [roomA, roomB, roomC],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [roomA.ClimateEntityId] = TestDataFactory.CreateClimateState(roomA.ClimateEntityId, ThermostatPresetMode.Home),
                [roomB.ClimateEntityId] = TestDataFactory.CreateClimateState(roomB.ClimateEntityId, ThermostatPresetMode.Home),
                [roomC.ClimateEntityId] = TestDataFactory.CreateClimateState(roomC.ClimateEntityId, ThermostatPresetMode.Home)
            },
            energyState: new EnergyState(2500, 0, 2000, 70, 0, 0));

        var result = _engine.Evaluate(input);

        result.AvailablePowerWatts.Should().Be(1500);
        result.RoomDecisions.Single(x => x.RoomName == "R1").TargetPreset.Should().Be(ThermostatPresetMode.Comfort);
        result.RoomDecisions.Single(x => x.RoomName == "R2").TargetPreset.Should().Be(ThermostatPresetMode.Comfort);
        result.RoomDecisions.Single(x => x.RoomName == "R3").TargetPreset.Should().NotBe(ThermostatPresetMode.Comfort);
    }

    [Fact]
    public void Should_RemoveStorageFromLowestPriority_WhenGridImportAboveLimit()
    {
        var roomHighPriority = TestDataFactory.CreateRoom("High", "climate.high", 500, 1);
        var roomLowPriority = TestDataFactory.CreateRoom("Low", "climate.low", 500, 10);

        var input = BuildInput(
            rooms: [roomHighPriority, roomLowPriority],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [roomHighPriority.ClimateEntityId] = TestDataFactory.CreateClimateState(roomHighPriority.ClimateEntityId, ThermostatPresetMode.Comfort),
                [roomLowPriority.ClimateEntityId] = TestDataFactory.CreateClimateState(roomLowPriority.ClimateEntityId, ThermostatPresetMode.Comfort)
            },
            energyState: new EnergyState(0, 300, 1200, 80, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single(x => x.RoomName == "Low").TargetPreset.Should().NotBe(ThermostatPresetMode.Comfort);
    }

    [Fact]
    public void Should_RemoveStorageFromLowestPriority_WhenBatteryDischargeAboveLimit()
    {
        var roomHighPriority = TestDataFactory.CreateRoom("High", "climate.high", 500, 1);
        var roomLowPriority = TestDataFactory.CreateRoom("Low", "climate.low", 500, 10);

        var input = BuildInput(
            rooms: [roomHighPriority, roomLowPriority],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [roomHighPriority.ClimateEntityId] = TestDataFactory.CreateClimateState(roomHighPriority.ClimateEntityId, ThermostatPresetMode.Comfort),
                [roomLowPriority.ClimateEntityId] = TestDataFactory.CreateClimateState(roomLowPriority.ClimateEntityId, ThermostatPresetMode.Comfort)
            },
            energyState: new EnergyState(0, 100, 1200, 80, 0, 150));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single(x => x.RoomName == "Low").TargetPreset.Should().NotBe(ThermostatPresetMode.Comfort);
    }

    [Fact]
    public void Should_BlockStorage_WhenBatterySocBelowThreshold()
    {
        var room = TestDataFactory.CreateRoom("Salon", "climate.salon", 500, 1);
        var input = BuildInput(
            rooms: [room],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [room.ClimateEntityId] = TestDataFactory.CreateClimateState(room.ClimateEntityId, ThermostatPresetMode.Home)
            },
            energyState: new EnergyState(2000, 0, 1500, 20, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single().TargetPreset.Should().NotBe(ThermostatPresetMode.Comfort);
    }

    [Fact]
    public void Should_SetBedroomToSleep_WhenNightModeIsActive()
    {
        var bedroom = TestDataFactory.CreateRoom(
            "Sypialnia",
            "climate.sypialnia",
            400,
            1,
            roomType: RoomType.Bedroom,
            storagePreset: ThermostatPresetMode.Activity,
            nightPreset: ThermostatPresetMode.Sleep);

        var input = BuildInput(
            rooms: [bedroom],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [bedroom.ClimateEntityId] = TestDataFactory.CreateClimateState(bedroom.ClimateEntityId, ThermostatPresetMode.Home)
            },
            energyState: new EnergyState(2000, 0, 1500, 70, 0, 0),
            scheduleState: new ScheduleEvaluationState(true, null, null, null, null));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single().TargetPreset.Should().Be(ThermostatPresetMode.Sleep);
    }

    [Fact]
    public void Should_SetAwayForAll_WhenVacationModeIsActive()
    {
        var roomA = TestDataFactory.CreateRoom("A", "climate.a", 300, 1);
        var roomB = TestDataFactory.CreateRoom("B", "climate.b", 300, 2);

        var input = BuildInput(
            rooms: [roomA, roomB],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [roomA.ClimateEntityId] = TestDataFactory.CreateClimateState(roomA.ClimateEntityId, ThermostatPresetMode.Home),
                [roomB.ClimateEntityId] = TestDataFactory.CreateClimateState(roomB.ClimateEntityId, ThermostatPresetMode.Home)
            },
            presenceMode: PresenceMode.Vacation,
            energyState: new EnergyState(0, 0, 0, 80, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Should().OnlyContain(x => x.TargetPreset == ThermostatPresetMode.Away);
    }

    [Fact]
    public void Should_SetNoPowerPresetForAll_WhenNoGridPower()
    {
        var roomA = TestDataFactory.CreateRoom("A", "climate.a", 300, 1, noPowerPreset: ThermostatPresetMode.Eco);
        var roomB = TestDataFactory.CreateRoom("B", "climate.b", 300, 2, noPowerPreset: ThermostatPresetMode.None);

        var input = BuildInput(
            rooms: [roomA, roomB],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [roomA.ClimateEntityId] = TestDataFactory.CreateClimateState(roomA.ClimateEntityId, ThermostatPresetMode.Home),
                [roomB.ClimateEntityId] = TestDataFactory.CreateClimateState(roomB.ClimateEntityId, ThermostatPresetMode.Home)
            },
            energyMode: EnergyMode.NoGridPower);

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single(x => x.RoomName == "A").TargetPreset.Should().Be(ThermostatPresetMode.Eco);
        result.RoomDecisions.Single(x => x.RoomName == "B").TargetPreset.Should().Be(ThermostatPresetMode.None);
    }

    [Fact]
    public void Should_NotUseUnsupportedActivityPreset_WhenClimateDoesNotSupportIt()
    {
        var room = TestDataFactory.CreateRoom(
            "Buffer",
            "climate.buffer",
            400,
            1,
            storagePreset: ThermostatPresetMode.Activity,
            defaultPreset: ThermostatPresetMode.Home,
            ecoPreset: ThermostatPresetMode.Eco);

        var supported = new HashSet<ThermostatPresetMode> { ThermostatPresetMode.Home, ThermostatPresetMode.Eco };

        var input = BuildInput(
            rooms: [room],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [room.ClimateEntityId] = TestDataFactory.CreateClimateState(room.ClimateEntityId, ThermostatPresetMode.Eco, supported)
            },
            energyState: new EnergyState(2000, 0, 1500, 80, 0, 0));

        var result = _engine.Evaluate(input);
        var decision = result.RoomDecisions.Single();

        decision.TargetPreset.Should().NotBe(ThermostatPresetMode.Activity);
        decision.CommandRequest.Should().NotBeNull();
        decision.CommandRequest!.Data["preset_mode"].Should().NotBe("activity");
    }

    [Fact]
    public void Should_NotSendCommand_WhenPresetIsAlreadyCorrect()
    {
        var room = TestDataFactory.CreateRoom("Salon", "climate.salon", 500, 1, defaultPreset: ThermostatPresetMode.Home);
        var input = BuildInput(
            rooms: [room],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [room.ClimateEntityId] = TestDataFactory.CreateClimateState(room.ClimateEntityId, ThermostatPresetMode.Home)
            },
            energyState: new EnergyState(0, 0, 0, 80, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single().CommandSent.Should().BeFalse();
    }

    [Fact]
    public void Should_NotSendCommand_WhenCommandCooldownIsActive()
    {
        var room = TestDataFactory.CreateRoom("Salon", "climate.salon", 500, 1, commandCooldownSeconds: 120);
        var input = BuildInput(
            rooms: [room],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [room.ClimateEntityId] = TestDataFactory.CreateClimateState(
                    room.ClimateEntityId,
                    ThermostatPresetMode.Eco,
                    lastPresetCommandAtUtc: _clock.UtcNow.AddSeconds(-30))
            },
            energyState: new EnergyState(0, 0, 0, 80, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single().CommandSent.Should().BeFalse();
    }

    [Fact]
    public void Should_KeepStorage_WhenMinOnTimeIsActive()
    {
        var room = TestDataFactory.CreateRoom(
            "Salon",
            "climate.salon",
            500,
            1,
            minOnTimeSeconds: 3600,
            storagePreset: ThermostatPresetMode.Comfort);

        var input = BuildInput(
            rooms: [room],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [room.ClimateEntityId] = TestDataFactory.CreateClimateState(
                    room.ClimateEntityId,
                    ThermostatPresetMode.Comfort,
                    lastStorageChangeAtUtc: _clock.UtcNow.AddMinutes(-5))
            },
            energyMode: EnergyMode.ExpensivePower,
            energyState: new EnergyState(0, 0, 0, 80, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single().TargetPreset.Should().Be(ThermostatPresetMode.Comfort);
    }

    [Fact]
    public void Should_BlockStorageStart_WhenMinOffTimeIsActive()
    {
        var room = TestDataFactory.CreateRoom(
            "Salon",
            "climate.salon",
            500,
            1,
            minOffTimeSeconds: 3600,
            storagePreset: ThermostatPresetMode.Comfort);

        var input = BuildInput(
            rooms: [room],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [room.ClimateEntityId] = TestDataFactory.CreateClimateState(
                    room.ClimateEntityId,
                    ThermostatPresetMode.Home,
                    lastStorageChangeAtUtc: _clock.UtcNow.AddMinutes(-10))
            },
            energyState: new EnergyState(2000, 0, 1500, 80, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Single().TargetPreset.Should().NotBe(ThermostatPresetMode.Comfort);
    }

    [Fact]
    public void Should_AssignReasonToEveryDecision()
    {
        var roomA = TestDataFactory.CreateRoom("A", "climate.a", 500, 1);
        var roomB = TestDataFactory.CreateRoom("B", "climate.b", 500, 2);

        var input = BuildInput(
            rooms: [roomA, roomB],
            climates: new Dictionary<string, ClimateEntityState>
            {
                [roomA.ClimateEntityId] = TestDataFactory.CreateClimateState(roomA.ClimateEntityId, ThermostatPresetMode.Home),
                [roomB.ClimateEntityId] = TestDataFactory.CreateClimateState(roomB.ClimateEntityId, ThermostatPresetMode.Eco)
            },
            energyState: new EnergyState(0, 100, 300, 60, 0, 0));

        var result = _engine.Evaluate(input);

        result.RoomDecisions.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.Reason.Message));
    }

    private static PowerManagerInput BuildInput(
        IReadOnlyCollection<RoomConfiguration> rooms,
        IReadOnlyDictionary<string, ClimateEntityState> climates,
        EnergyState? energyState = null,
        EnergyMode energyMode = EnergyMode.NormalPower,
        PresenceMode presenceMode = PresenceMode.EveryoneHome,
        ScheduleEvaluationState? scheduleState = null,
        PowerManagerSettings? settings = null)
    {
        return new PowerManagerInput(
            rooms,
            climates,
            energyState ?? new EnergyState(0, 0, 0, 80, 0, 0),
            energyMode,
            presenceMode,
            scheduleState ?? new ScheduleEvaluationState(false, null, null, null, null),
            settings ?? TestDataFactory.DefaultSettings());
    }
}
