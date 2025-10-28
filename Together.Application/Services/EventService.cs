using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class EventService : IEventService
{
    private readonly ISharedEventRepository _eventRepository;
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;

    public EventService(
        ISharedEventRepository eventRepository,
        ICoupleConnectionRepository connectionRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository)
    {
        _eventRepository = eventRepository;
        _connectionRepository = connectionRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<SharedEventDto> CreateEventAsync(Guid userId, CreateEventDto dto)
    {
        // Get user's couple connection
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            throw new BusinessRuleViolationException("User must have an active couple connection to create events");
        }

        // Create the event
        var sharedEvent = new SharedEvent(
            connection.Id,
            userId,
            dto.Title,
            dto.EventDate,
            dto.Description,
            dto.Recurrence ?? "none"
        );

        await _eventRepository.AddAsync(sharedEvent);

        // Generate recurring events if needed
        if (dto.Recurrence != "none")
        {
            await GenerateRecurringEventsForEventAsync(sharedEvent);
        }

        return await MapToDto(sharedEvent);
    }

    public async Task<SharedEventDto> UpdateEventAsync(Guid userId, UpdateEventDto dto)
    {
        var sharedEvent = await _eventRepository.GetByIdAsync(dto.Id);
        if (sharedEvent == null)
        {
            throw new NotFoundException(nameof(SharedEvent), dto.Id);
        }

        // Verify user has access to this event
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null || sharedEvent.ConnectionId != connection.Id)
        {
            throw new BusinessRuleViolationException("User does not have access to this event");
        }

        sharedEvent.Update(dto.Title, dto.EventDate, dto.Description, dto.Recurrence ?? "none");
        await _eventRepository.UpdateAsync(sharedEvent);

        return await MapToDto(sharedEvent);
    }

    public async Task DeleteEventAsync(Guid userId, Guid eventId)
    {
        var sharedEvent = await _eventRepository.GetByIdAsync(eventId);
        if (sharedEvent == null)
        {
            throw new NotFoundException(nameof(SharedEvent), eventId);
        }

        // Verify user has access to this event
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null || sharedEvent.ConnectionId != connection.Id)
        {
            throw new BusinessRuleViolationException("User does not have access to this event");
        }

        await _eventRepository.DeleteAsync(eventId);
    }

    public async Task<SharedEventDto?> GetEventByIdAsync(Guid eventId)
    {
        var sharedEvent = await _eventRepository.GetByIdAsync(eventId);
        if (sharedEvent == null)
        {
            return null;
        }

        return await MapToDto(sharedEvent);
    }

    public async Task<IEnumerable<SharedEventDto>> GetUserEventsAsync(Guid userId)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<SharedEventDto>();
        }

        var events = await _eventRepository.GetByConnectionIdAsync(connection.Id);
        var eventDtos = new List<SharedEventDto>();

        foreach (var evt in events)
        {
            eventDtos.Add(await MapToDto(evt));
        }

        return eventDtos;
    }

    public async Task<IEnumerable<SharedEventDto>> GetUpcomingEventsAsync(Guid userId, int limit = 10)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<SharedEventDto>();
        }

        var events = await _eventRepository.GetUpcomingEventsAsync(connection.Id, DateTime.UtcNow, limit);
        var eventDtos = new List<SharedEventDto>();

        foreach (var evt in events)
        {
            eventDtos.Add(await MapToDto(evt));
        }

        return eventDtos;
    }

    public async Task<IEnumerable<SharedEventDto>> GetEventsByMonthAsync(Guid userId, int year, int month)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<SharedEventDto>();
        }

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var events = await _eventRepository.GetEventsByDateRangeAsync(connection.Id, startDate, endDate);
        var eventDtos = new List<SharedEventDto>();

        foreach (var evt in events)
        {
            eventDtos.Add(await MapToDto(evt));
        }

        return eventDtos;
    }

    public async Task<int> GetDaysTogetherAsync(Guid userId)
    {
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return 0;
        }

        var daysTogether = (DateTime.UtcNow.Date - connection.RelationshipStartDate.Date).Days;
        return daysTogether;
    }

    public Task ProcessEventRemindersAsync()
    {
        // This would typically be called by a background service
        // Get all events happening in the next 24 hours
        var reminderTime = DateTime.UtcNow.AddHours(24);
        
        // Note: This is a simplified implementation
        // In production, you'd want to track which reminders have been sent
        // to avoid duplicate notifications
        
        return Task.CompletedTask;
    }

    public async Task GenerateRecurringEventsAsync(Guid connectionId)
    {
        var events = await _eventRepository.GetByConnectionIdAsync(connectionId);
        
        foreach (var evt in events.Where(e => e.Recurrence != "none"))
        {
            await GenerateRecurringEventsForEventAsync(evt);
        }
    }

    private async Task GenerateRecurringEventsForEventAsync(SharedEvent baseEvent)
    {
        // Generate recurring events for the next 12 months
        var endDate = DateTime.UtcNow.AddMonths(12);
        var currentDate = baseEvent.EventDate;

        while (currentDate <= endDate)
        {
            currentDate = baseEvent.Recurrence switch
            {
                "daily" => currentDate.AddDays(1),
                "weekly" => currentDate.AddDays(7),
                "monthly" => currentDate.AddMonths(1),
                "yearly" => currentDate.AddYears(1),
                _ => endDate.AddDays(1) // Break the loop
            };

            if (currentDate <= endDate && currentDate > baseEvent.EventDate)
            {
                var recurringEvent = new SharedEvent(
                    baseEvent.ConnectionId,
                    baseEvent.CreatedBy,
                    baseEvent.Title ?? string.Empty,
                    currentDate,
                    baseEvent.Description,
                    "none" // Recurring instances are marked as "none" to avoid infinite recursion
                );

                await _eventRepository.AddAsync(recurringEvent);
            }
        }
    }

    private async Task<SharedEventDto> MapToDto(SharedEvent sharedEvent)
    {
        var creator = sharedEvent.Creator ?? await _userRepository.GetByIdAsync(sharedEvent.CreatedBy);
        if (creator == null)
        {
            throw new NotFoundException(nameof(User), sharedEvent.CreatedBy);
        }

        var creatorDto = new UserDto(
            creator.Id,
            creator.Username,
            creator.Email.Value,
            creator.ProfilePictureUrl,
            creator.Bio
        );

        // Check if event is within 24 hours for reminder
        var hasReminder = (sharedEvent.EventDate - DateTime.UtcNow).TotalHours <= 24 && 
                         sharedEvent.EventDate > DateTime.UtcNow;

        return new SharedEventDto(
            sharedEvent.Id,
            sharedEvent.ConnectionId,
            sharedEvent.Title ?? string.Empty,
            sharedEvent.Description,
            sharedEvent.EventDate,
            sharedEvent.Recurrence ?? "none",
            creatorDto,
            sharedEvent.CreatedAt,
            hasReminder
        );
    }
}
