namespace Together.Application.DTOs;

public record UpdateEventDto(
    Guid Id,
    string Title,
    DateTime EventDate,
    string? Description,
    string Recurrence
);
