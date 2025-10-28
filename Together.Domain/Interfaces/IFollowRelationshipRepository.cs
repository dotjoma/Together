using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IFollowRelationshipRepository
{
    Task<FollowRelationship?> GetByIdAsync(Guid id);
    Task<FollowRelationship?> GetByFollowerAndFollowingAsync(Guid followerId, Guid followingId);
    Task<IEnumerable<FollowRelationship>> GetFollowersAsync(Guid userId);
    Task<IEnumerable<FollowRelationship>> GetFollowingAsync(Guid userId);
    Task<IEnumerable<FollowRelationship>> GetPendingRequestsAsync(Guid userId);
    Task AddAsync(FollowRelationship followRelationship);
    Task UpdateAsync(FollowRelationship followRelationship);
    Task DeleteAsync(Guid id);
    Task<int> GetFollowerCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
}
