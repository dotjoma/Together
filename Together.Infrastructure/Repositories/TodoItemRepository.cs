using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class TodoItemRepository : ITodoItemRepository
{
    private readonly TogetherDbContext _context;

    public TodoItemRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id)
    {
        return await _context.TodoItems
            .Include(t => t.AssignedUser)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TodoItem>> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _context.TodoItems
            .Include(t => t.AssignedUser)
            .Include(t => t.Creator)
            .Where(t => t.ConnectionId == connectionId)
            .OrderBy(t => t.Completed)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetByConnectionIdAndTagsAsync(Guid connectionId, List<string> tags)
    {
        var query = _context.TodoItems
            .Include(t => t.AssignedUser)
            .Include(t => t.Creator)
            .Where(t => t.ConnectionId == connectionId);

        if (tags != null && tags.Any())
        {
            query = query.Where(t => t.Tags.Any(tag => tags.Contains(tag)));
        }

        return await query
            .OrderBy(t => t.Completed)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetOverdueItemsAsync(Guid connectionId)
    {
        var now = DateTime.UtcNow;
        return await _context.TodoItems
            .Include(t => t.AssignedUser)
            .Include(t => t.Creator)
            .Where(t => t.ConnectionId == connectionId && 
                       !t.Completed && 
                       t.DueDate.HasValue && 
                       t.DueDate.Value < now)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task AddAsync(TodoItem todoItem)
    {
        await _context.TodoItems.AddAsync(todoItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TodoItem todoItem)
    {
        _context.TodoItems.Update(todoItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem != null)
        {
            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
        }
    }
}
