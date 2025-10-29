namespace Together.Application.DTOs;

public record MoodEntryDto(
    Guid Id,
    Guid UserId,
    string Mood,
    string? Notes,
    DateTime Timestamp
);
