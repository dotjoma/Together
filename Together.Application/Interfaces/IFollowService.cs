using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IFollowService
{
    Task<FollowRelationshipDto> SendFollowRequestAsync(Guid followerId, Guid followingId);
    Task<FollowRelationshipDto> AcceptFollowRequestAsync(Guid requestId);
    Task RejectFollowRequestAsync(Guid requestId);
    Task UnfollowAsync(Guid followerId, Guid followingId);
    Task<IEnumerable<FollowRelationshipDto>> GetFollowersAsync(Guid userId);
    Task<IEnumerable<FollowRelationshipDto>> GetFollowingAsync(Guid userId);
    Task<IEnumerable<FollowRelationshipDto>> GetPendingRequestsAsync(Guid userId);
    Task<int> GetFollowerCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
    Task<string> GetFollowStatusAsync(Guid currentUserId, Guid targetUserId);
}
