using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRelationshipRepository _followRepository;
    private readonly IUserRepository _userRepository;

    public FollowService(
        IFollowRelationshipRepository followRepository,
        IUserRepository userRepository)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
    }

    public async Task<FollowRelationshipDto> SendFollowRequestAsync(Guid followerId, Guid followingId)
    {
        // Validate users exist
        var follower = await _userRepository.GetByIdAsync(followerId)
            ?? throw new NotFoundException(nameof(User), followerId);
        
        var following = await _userRepository.GetByIdAsync(followingId)
            ?? throw new NotFoundException(nameof(User), followingId);

        // Check if relationship already exists
        var existingRelationship = await _followRepository.GetByFollowerAndFollowingAsync(followerId, followingId);
        if (existingRelationship != null)
        {
            if (existingRelationship.Status == "pending")
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "FollowRequest", new[] { "A follow request is already pending." } }
                });
            
            if (existingRelationship.Status == "accepted")
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "FollowRequest", new[] { "You are already following this user." } }
                });
        }

        // Create new follow relationship
        var followRelationship = new FollowRelationship(followerId, followingId);
        await _followRepository.AddAsync(followRelationship);

        return MapToDto(followRelationship, follower, following);
    }

    public async Task<FollowRelationshipDto> AcceptFollowRequestAsync(Guid requestId)
    {
        var followRelationship = await _followRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException(nameof(FollowRelationship), requestId);

        if (!followRelationship.IsPending())
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "FollowRequest", new[] { "This follow request is not pending." } }
            });

        followRelationship.Accept();
        await _followRepository.UpdateAsync(followRelationship);

        return MapToDto(followRelationship);
    }

    public async Task RejectFollowRequestAsync(Guid requestId)
    {
        var followRelationship = await _followRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException(nameof(FollowRelationship), requestId);

        if (!followRelationship.IsPending())
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "FollowRequest", new[] { "This follow request is not pending." } }
            });

        followRelationship.Reject();
        await _followRepository.DeleteAsync(requestId);
    }

    public async Task UnfollowAsync(Guid followerId, Guid followingId)
    {
        var followRelationship = await _followRepository.GetByFollowerAndFollowingAsync(followerId, followingId)
            ?? throw new NotFoundException(nameof(FollowRelationship), $"{followerId}-{followingId}");

        await _followRepository.DeleteAsync(followRelationship.Id);
    }

    public async Task<IEnumerable<FollowRelationshipDto>> GetFollowersAsync(Guid userId)
    {
        var followers = await _followRepository.GetFollowersAsync(userId);
        return followers.Select(MapToDto);
    }

    public async Task<IEnumerable<FollowRelationshipDto>> GetFollowingAsync(Guid userId)
    {
        var following = await _followRepository.GetFollowingAsync(userId);
        return following.Select(MapToDto);
    }

    public async Task<IEnumerable<FollowRelationshipDto>> GetPendingRequestsAsync(Guid userId)
    {
        var pendingRequests = await _followRepository.GetPendingRequestsAsync(userId);
        return pendingRequests.Select(MapToDto);
    }

    public async Task<int> GetFollowerCountAsync(Guid userId)
    {
        return await _followRepository.GetFollowerCountAsync(userId);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _followRepository.GetFollowingCountAsync(userId);
    }

    public async Task<string> GetFollowStatusAsync(Guid currentUserId, Guid targetUserId)
    {
        if (currentUserId == targetUserId)
            return "self";

        var relationship = await _followRepository.GetByFollowerAndFollowingAsync(currentUserId, targetUserId);
        
        if (relationship == null)
            return "none";
        
        return relationship.Status;
    }

    private FollowRelationshipDto MapToDto(FollowRelationship followRelationship)
    {
        return new FollowRelationshipDto(
            followRelationship.Id,
            MapUserToDto(followRelationship.Follower),
            MapUserToDto(followRelationship.Following),
            followRelationship.Status,
            followRelationship.CreatedAt,
            followRelationship.AcceptedAt
        );
    }

    private FollowRelationshipDto MapToDto(FollowRelationship followRelationship, User follower, User following)
    {
        return new FollowRelationshipDto(
            followRelationship.Id,
            MapUserToDto(follower),
            MapUserToDto(following),
            followRelationship.Status,
            followRelationship.CreatedAt,
            followRelationship.AcceptedAt
        );
    }

    private UserDto MapUserToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.ProfilePictureUrl,
            user.Bio
        );
    }
}
