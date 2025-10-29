namespace Together.Application.DTOs;

/// <summary>
/// Data transfer object for mood entries
/// </summary>
public record MoodEntryDto(
    Guid Id,
    Guid UserId,
    string Mood,
    string? Notes,
    DateTime Timestamp
);
