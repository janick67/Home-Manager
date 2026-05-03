using FluentAssertions;
using HomeManager.Application.Services;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;
using HomeManager.Tests.Unit.Helpers;

namespace HomeManager.Tests.Unit;

public sealed class ScheduleEvaluatorTests
{
    [Fact]
    public void Should_ApplyOneTimeSchedule_WhenCurrentTimeInsideRange()
    {
        var clock = new TestClock { UtcNow = new DateTimeOffset(2026, 5, 6, 12, 0, 0, TimeSpan.Zero) };
        var evaluator = new ScheduleEvaluator(clock, new OverridePriorityResolver());

        var schedule = new ScheduleDefinition(
            Guid.NewGuid(),
            "Vacation One Time",
            true,
            ScheduleType.OneTime,
            ScheduleTargetType.Presence,
            PresenceMode.Vacation,
            null,
            clock.UtcNow.AddHours(-1),
            clock.UtcNow.AddHours(2),
            new HashSet<DayOfWeek>(),
            null,
            null,
            1);

        var state = evaluator.Evaluate([schedule], [], false);

        state.ScheduledPresenceMode.Should().Be(PresenceMode.Vacation);
    }

    [Fact]
    public void Should_ApplyRecurringSchedule_MondayToFridaySevenToFifteen()
    {
        var clock = new TestClock { UtcNow = new DateTimeOffset(2026, 5, 4, 9, 0, 0, TimeSpan.Zero) }; // Monday
        var evaluator = new ScheduleEvaluator(clock, new OverridePriorityResolver());

        var schedule = new ScheduleDefinition(
            Guid.NewGuid(),
            "Office Hours",
            true,
            ScheduleType.Recurring,
            ScheduleTargetType.Presence,
            PresenceMode.WorkingInOffice,
            null,
            null,
            null,
            new HashSet<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            },
            new TimeOnly(7, 0),
            new TimeOnly(15, 0),
            1);

        var state = evaluator.Evaluate([schedule], [], false);

        state.ScheduledPresenceMode.Should().Be(PresenceMode.WorkingInOffice);
    }

    [Fact]
    public void Should_ApplyManualOverrideOverSchedule()
    {
        var clock = new TestClock { UtcNow = new DateTimeOffset(2026, 5, 4, 9, 0, 0, TimeSpan.Zero) };
        var evaluator = new ScheduleEvaluator(clock, new OverridePriorityResolver());

        var schedule = new ScheduleDefinition(
            Guid.NewGuid(),
            "Office Hours",
            true,
            ScheduleType.Recurring,
            ScheduleTargetType.Presence,
            PresenceMode.WorkingInOffice,
            null,
            null,
            null,
            new HashSet<DayOfWeek> { DayOfWeek.Monday },
            new TimeOnly(7, 0),
            new TimeOnly(15, 0),
            1);

        var manual = new ManualOverrideDefinition(
            Guid.NewGuid(),
            "Nobody Home 2h",
            true,
            ScheduleTargetType.Presence,
            PresenceMode.NobodyHome,
            null,
            clock.UtcNow.AddHours(-1),
            clock.UtcNow.AddHours(1),
            10);

        var state = evaluator.Evaluate([schedule], [manual], false);

        state.ScheduledPresenceMode.Should().Be(PresenceMode.WorkingInOffice);
        state.ManualPresenceOverride.Should().Be(PresenceMode.NobodyHome);
    }
}
