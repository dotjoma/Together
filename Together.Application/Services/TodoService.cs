using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class TodoService : ITodoService
{
    private readonly ITodoItemRepository _todoRepository;
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IUserRepository _userRepository;

    public TodoService(
        ITodoItemRepository todoRepository,
        ICoupleConnectionRepository connectionRepository,
        IUserRepository userRepository)
    {
        _todoRepository = todoRepository;
        _connectionRepository = connectionRepository;
        _userRepository = userRepository;
    }

    public async Task<TodoItemDto> CreateTodoItemAsync(Guid userId, CreateTodoItemDto dto)
    {
        // Validate user has a couple connection
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            throw new BusinessRuleViolationException("User must have an active couple connection to create todo items");
        }

        // Validate title
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Title), new[] { "Title is required" } }
            });
        }

        // Validate assigned user is part of the connection
        if (dto.AssignedTo.HasValue)
        {
            var partnerId = connection.User1Id == userId ? connection.User2Id : connection.User1Id;
            if (dto.AssignedTo.Value != userId && dto.AssignedTo.Value != partnerId)
            {
                throw new BusinessRuleViolationException("Todo item can only be assigned to users in the couple connection");
            }
        }

        // Create todo item
        var todoItem = new TodoItem(
            connection.Id,
            userId,
            dto.Title,
            dto.Description,
            dto.AssignedTo,
            dto.DueDate
        );

        // Add tags
        if (dto.Tags != null)
        {
            foreach (var tag in dto.Tags)
            {
                todoItem.AddTag(tag);
            }
        }

        await _todoRepository.AddAsync(todoItem);

        // TODO: Notify partner via real-time service when implemented

        return await MapToDto(todoItem);
    }

    public async Task<TodoItemDto> MarkAsCompleteAsync(Guid todoId, Guid userId)
    {
        var todoItem = await GetAndValidateTodoItem(todoId, userId);

        todoItem.MarkAsComplete();
        await _todoRepository.UpdateAsync(todoItem);

        // TODO: Notify partner via real-time service when implemented

        return await MapToDto(todoItem);
    }

    public async Task<TodoItemDto> MarkAsIncompleteAsync(Guid todoId, Guid userId)
    {
        var todoItem = await GetAndValidateTodoItem(todoId, userId);

        todoItem.MarkAsIncomplete();
        await _todoRepository.UpdateAsync(todoItem);

        return await MapToDto(todoItem);
    }

    public async Task<TodoItemDto> UpdateTodoItemAsync(Guid todoId, Guid userId, UpdateTodoItemDto dto)
    {
        var todoItem = await GetAndValidateTodoItem(todoId, userId);

        // Validate title
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Title), new[] { "Title is required" } }
            });
        }

        // Validate assigned user is part of the connection
        if (dto.AssignedTo.HasValue)
        {
            var connection = await _connectionRepository.GetByIdAsync(todoItem.ConnectionId);
            if (connection != null)
            {
                var partnerId = connection.User1Id == userId ? connection.User2Id : connection.User1Id;
                if (dto.AssignedTo.Value != userId && dto.AssignedTo.Value != partnerId)
                {
                    throw new BusinessRuleViolationException("Todo item can only be assigned to users in the couple connection");
                }
            }
        }

        // Update todo item
        todoItem.Update(dto.Title, dto.Description, dto.AssignedTo, dto.DueDate);

        // Update tags
        if (dto.Tags != null)
        {
            // Clear existing tags and add new ones
            var existingTags = todoItem.Tags.ToList();
            foreach (var tag in existingTags)
            {
                todoItem.RemoveTag(tag);
            }

            foreach (var tag in dto.Tags)
            {
                todoItem.AddTag(tag);
            }
        }

        await _todoRepository.UpdateAsync(todoItem);

        return await MapToDto(todoItem);
    }

    public async Task DeleteTodoItemAsync(Guid todoId, Guid userId)
    {
        var todoItem = await GetAndValidateTodoItem(todoId, userId);
        await _todoRepository.DeleteAsync(todoId);
    }

    public async Task<IEnumerable<TodoItemDto>> GetTodoItemsAsync(Guid userId)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<TodoItemDto>();
        }

        var todoItems = await _todoRepository.GetByConnectionIdAsync(connection.Id);
        var dtos = new List<TodoItemDto>();

        foreach (var item in todoItems)
        {
            dtos.Add(await MapToDto(item));
        }

        return dtos;
    }

    public async Task<IEnumerable<TodoItemDto>> GetTodoItemsByTagsAsync(Guid userId, List<string> tags)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<TodoItemDto>();
        }

        var todoItems = await _todoRepository.GetByConnectionIdAndTagsAsync(connection.Id, tags);
        var dtos = new List<TodoItemDto>();

        foreach (var item in todoItems)
        {
            dtos.Add(await MapToDto(item));
        }

        return dtos;
    }

    public async Task<IEnumerable<TodoItemDto>> GetOverdueItemsAsync(Guid userId)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<TodoItemDto>();
        }

        var todoItems = await _todoRepository.GetOverdueItemsAsync(connection.Id);
        var dtos = new List<TodoItemDto>();

        foreach (var item in todoItems)
        {
            dtos.Add(await MapToDto(item));
        }

        return dtos;
    }

    public async Task<TodoItemDto?> GetTodoItemByIdAsync(Guid todoId, Guid userId)
    {
        var todoItem = await _todoRepository.GetByIdAsync(todoId);
        if (todoItem == null)
        {
            return null;
        }

        // Validate user has access to this todo item
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null || todoItem.ConnectionId != connection.Id)
        {
            throw new BusinessRuleViolationException("User does not have access to this todo item");
        }

        return await MapToDto(todoItem);
    }

    private async Task<TodoItem> GetAndValidateTodoItem(Guid todoId, Guid userId)
    {
        var todoItem = await _todoRepository.GetByIdAsync(todoId);
        if (todoItem == null)
        {
            throw new NotFoundException(nameof(TodoItem), todoId);
        }

        // Validate user has access to this todo item
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null || todoItem.ConnectionId != connection.Id)
        {
            throw new BusinessRuleViolationException("User does not have access to this todo item");
        }

        return todoItem;
    }

    private Task<TodoItemDto> MapToDto(TodoItem todoItem)
    {
        string? assignedToUsername = null;
        if (todoItem.AssignedTo.HasValue && todoItem.AssignedUser != null)
        {
            assignedToUsername = todoItem.AssignedUser.Username;
        }

        string createdByUsername = todoItem.Creator?.Username ?? "Unknown";

        var dto = new TodoItemDto(
            todoItem.Id,
            todoItem.ConnectionId,
            todoItem.Title ?? string.Empty,
            todoItem.Description,
            todoItem.AssignedTo,
            assignedToUsername,
            todoItem.CreatedBy,
            createdByUsername,
            todoItem.DueDate,
            todoItem.Completed,
            todoItem.CompletedAt,
            todoItem.Tags,
            todoItem.CreatedAt,
            todoItem.IsOverdue()
        );

        return Task.FromResult(dto);
    }
}
