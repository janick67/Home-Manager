using HomeManager.Application.Abstractions;

namespace HomeManager.Tests.Unit.Helpers;

internal sealed class TestClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
}
