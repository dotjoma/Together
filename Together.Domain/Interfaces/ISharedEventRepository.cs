using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface ISharedEventRepository
{
    Task<SharedEvent?> GetByIdAsync(Guid id);
    Task<IEnumerable<SharedEvent>> GetByConnectionIdAsync(Guid connectionId);
    Task<IEnumerable<SharedEvent>> GetUpcomingEventsAsync(Guid connectionId, DateTime from, int limit);
    Task<IEnumerable<SharedEvent>> GetEventsByDateRangeAsync(Guid connectionId, DateTime from, DateTime to);
    Task AddAsync(SharedEvent sharedEvent);
    Task UpdateAsync(SharedEvent sharedEvent);
    Task DeleteAsync(Guid id);
}
