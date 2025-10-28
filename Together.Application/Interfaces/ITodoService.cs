using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface ITodoService
{
    Task<TodoItemDto> CreateTodoItemAsync(Guid userId, CreateTodoItemDto dto);
    Task<TodoItemDto> MarkAsCompleteAsync(Guid todoId, Guid userId);
    Task<TodoItemDto> MarkAsIncompleteAsync(Guid todoId, Guid userId);
    Task<TodoItemDto> UpdateTodoItemAsync(Guid todoId, Guid userId, UpdateTodoItemDto dto);
    Task DeleteTodoItemAsync(Guid todoId, Guid userId);
    Task<IEnumerable<TodoItemDto>> GetTodoItemsAsync(Guid userId);
    Task<IEnumerable<TodoItemDto>> GetTodoItemsByTagsAsync(Guid userId, List<string> tags);
    Task<IEnumerable<TodoItemDto>> GetOverdueItemsAsync(Guid userId);
    Task<TodoItemDto?> GetTodoItemByIdAsync(Guid todoId, Guid userId);
}
