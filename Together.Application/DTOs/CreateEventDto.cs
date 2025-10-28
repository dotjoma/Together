namespace Together.Application.DTOs;

public record CreateEventDto(
    string Title,
    DateTime EventDate,
    string? Description,
    string Recurrence
);
