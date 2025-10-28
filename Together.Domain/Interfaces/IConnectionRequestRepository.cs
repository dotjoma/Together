using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IConnectionRequestRepository
{
    Task<ConnectionRequest?> GetByIdAsync(Guid id);
    Task<IEnumerable<ConnectionRequest>> GetPendingRequestsForUserAsync(Guid userId);
    Task<ConnectionRequest?> GetPendingRequestBetweenUsersAsync(Guid fromUserId, Guid toUserId);
    Task AddAsync(ConnectionRequest request);
    Task UpdateAsync(ConnectionRequest request);
    Task DeleteAsync(Guid id);
}
