namespace HomeManager.Application.Contracts;

public sealed record LogEntryDto(
    DateTimeOffset TimestampUtc,
    string Level,
    string Message,
    string? Details);
