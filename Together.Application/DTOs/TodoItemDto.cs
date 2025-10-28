namespace Together.Application.DTOs;

public record TodoItemDto(
    Guid Id,
    Guid ConnectionId,
    string Title,
    string? Description,
    Guid? AssignedTo,
    string? AssignedToUsername,
    Guid CreatedBy,
    string CreatedByUsername,
    DateTime? DueDate,
    bool Completed,
    DateTime? CompletedAt,
    List<string> Tags,
    DateTime CreatedAt,
    bool IsOverdue
);
