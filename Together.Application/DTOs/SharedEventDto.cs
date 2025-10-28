namespace Together.Application.DTOs;

public record SharedEventDto(
    Guid Id,
    Guid ConnectionId,
    string Title,
    string? Description,
    DateTime EventDate,
    string Recurrence,
    UserDto Creator,
    DateTime CreatedAt,
    bool HasReminder
);
