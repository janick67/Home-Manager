using FluentAssertions;
using HomeManager.Application.Services;
using HomeManager.Domain.Enums;
using HomeManager.Domain.Models;

namespace HomeManager.Tests.Unit;

public sealed class OverridePriorityResolverTests
{
    private readonly OverridePriorityResolver _resolver = new();

    [Fact]
    public void Should_PrioritizeVacationOverWorkingInOffice()
    {
        var now = new DateTimeOffset(2026, 5, 4, 9, 0, 0, TimeSpan.Zero);
        var activeOverrides = new List<ManualOverrideDefinition>
        {
            new(
                Guid.NewGuid(),
                "Office",
                true,
                ScheduleTargetType.Presence,
                PresenceMode.WorkingInOffice,
                null,
                now.AddHours(-1),
                now.AddHours(1),
                100),
            new(
                Guid.NewGuid(),
                "Vacation",
                true,
                ScheduleTargetType.Presence,
                PresenceMode.Vacation,
                null,
                now.AddHours(-2),
                now.AddHours(2),
                1)
        };

        var result = _resolver.ResolvePresenceOverride(activeOverrides);

        result.Should().Be(PresenceMode.Vacation);
    }
}
