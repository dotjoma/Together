namespace Together.Application.DTOs;

public record UpdateTodoItemDto(
    string Title,
    string? Description,
    Guid? AssignedTo,
    DateTime? DueDate,
    List<string>? Tags
);
