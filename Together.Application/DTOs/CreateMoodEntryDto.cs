namespace Together.Application.DTOs;

public record CreateMoodEntryDto(
    string Mood,
    string? Notes
);
