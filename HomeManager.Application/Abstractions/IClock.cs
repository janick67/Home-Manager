namespace HomeManager.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
