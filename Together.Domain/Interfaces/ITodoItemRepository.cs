using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface ITodoItemRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetByConnectionIdAsync(Guid connectionId);
    Task<IEnumerable<TodoItem>> GetByConnectionIdAndTagsAsync(Guid connectionId, List<string> tags);
    Task<IEnumerable<TodoItem>> GetOverdueItemsAsync(Guid connectionId);
    Task AddAsync(TodoItem todoItem);
    Task UpdateAsync(TodoItem todoItem);
    Task DeleteAsync(Guid id);
}
