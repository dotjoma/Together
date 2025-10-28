namespace Together.Application.DTOs;

public record MoodEntryDto(
    Guid Id,
    string Mood,
    string? Notes,
    DateTime Timestamp
);
