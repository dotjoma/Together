using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IEventService
{
    Task<SharedEventDto> CreateEventAsync(Guid userId, CreateEventDto dto);
    Task<SharedEventDto> UpdateEventAsync(Guid userId, UpdateEventDto dto);
    Task DeleteEventAsync(Guid userId, Guid eventId);
    Task<SharedEventDto?> GetEventByIdAsync(Guid eventId);
    Task<IEnumerable<SharedEventDto>> GetUserEventsAsync(Guid userId);
    Task<IEnumerable<SharedEventDto>> GetUpcomingEventsAsync(Guid userId, int limit = 10);
    Task<IEnumerable<SharedEventDto>> GetEventsByMonthAsync(Guid userId, int year, int month);
    Task<int> GetDaysTogetherAsync(Guid userId);
    Task ProcessEventRemindersAsync();
    Task GenerateRecurringEventsAsync(Guid connectionId);
}
