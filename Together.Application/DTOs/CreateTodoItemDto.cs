namespace Together.Application.DTOs;

public record CreateTodoItemDto(
    string Title,
    string? Description,
    Guid? AssignedTo,
    DateTime? DueDate,
    List<string>? Tags
);
