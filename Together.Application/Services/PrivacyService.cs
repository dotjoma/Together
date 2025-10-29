using Microsoft.Extensions.Logging;
using Together.Application.Interfaces;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

/// <summary>
/// Implementation of privacy service for enforcing data isolation and access controls
/// </summary>
public class PrivacyService : IPrivacyService
{
    private readonly IUserRepository _userRepository;
    private readonly ICoupleConnectionRepository _coupleConnectionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRelationshipRepository _followRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<PrivacyService> _logger;

    public PrivacyService(
        IUserRepository userRepository,
        ICoupleConnectionRepository coupleConnectionRepository,
        IPostRepository postRepository,
        IFollowRelationshipRepository followRepository,
        IAuditLogger auditLogger,
        ILogger<PrivacyService> logger)
    {
        _userRepository = userRepository;
        _coupleConnectionRepository = coupleConnectionRepository;
        _postRepository = postRepository;
        _followRepository = followRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<bool> HasCoupleDataAccessAsync(Guid userId, Guid connectionId)
    {
        try
        {
            var connection = await _coupleConnectionRepository.GetByIdAsync(connectionId);
            if (connection == null)
            {
                _logger.LogWarning("Couple connection not found: {ConnectionId}", connectionId);
                return false;
            }

            var hasAccess = connection.User1Id == userId || connection.User2Id == userId;

            if (!hasAccess)
            {
                await _auditLogger.LogSecurityViolationAsync(
                    userId,
                    "UnauthorizedCoupleDataAccess",
                    $"User {userId} attempted to access couple data for connection {connectionId}");
            }
            else
            {
                await _auditLogger.LogDataAccessEventAsync(userId, "CoupleConnection", connectionId, "Access");
            }

            return hasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking couple data access for user {UserId} and connection {ConnectionId}", 
                userId, connectionId);
            return false;
        }
    }

    public async Task<bool> CanViewProfileAsync(Guid viewerId, Guid profileOwnerId)
    {
        try
        {
            // Users can always view their own profile
            if (viewerId == profileOwnerId)
            {
                return true;
            }

            var profileOwner = await _userRepository.GetByIdAsync(profileOwnerId);
            if (profileOwner == null)
            {
                return false;
            }

            // Check visibility settings
            switch (profileOwner.Visibility)
            {
                case ProfileVisibility.Public:
                    return true;

                case ProfileVisibility.Private:
                    // Only the user can view their private profile
                    return false;

                case ProfileVisibility.FriendsOnly:
                    // Check if viewer is following the profile owner (accepted follow relationship)
                    var followRelationship = await _followRepository.GetByFollowerAndFollowingAsync(viewerId, profileOwnerId);
                    var isFollowing = followRelationship != null && followRelationship.Status == "accepted";
                    
                    if (!isFollowing)
                    {
                        await _auditLogger.LogPrivacyEventAsync(
                            viewerId,
                            "ProfileAccessDenied",
                            $"User {viewerId} attempted to view friends-only profile {profileOwnerId}");
                    }
                    
                    return isFollowing;

                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking profile view permission for viewer {ViewerId} and owner {ProfileOwnerId}", 
                viewerId, profileOwnerId);
            return false;
        }
    }

    public async Task<bool> CanViewPostAsync(Guid viewerId, Guid postId)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return false;
            }

            // Users can always view their own posts
            if (post.AuthorId == viewerId)
            {
                return true;
            }

            // Check if viewer can view the author's profile
            return await CanViewProfileAsync(viewerId, post.AuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking post view permission for viewer {ViewerId} and post {PostId}", 
                viewerId, postId);
            return false;
        }
    }

    public async Task<IEnumerable<Guid>> FilterVisibleUsersAsync(Guid viewerId, IEnumerable<Guid> userIds)
    {
        var visibleUsers = new List<Guid>();

        foreach (var userId in userIds)
        {
            if (await CanViewProfileAsync(viewerId, userId))
            {
                visibleUsers.Add(userId);
            }
        }

        return visibleUsers;
    }

    public async Task<bool> IsPartOfConnectionAsync(Guid userId, Guid connectionId)
    {
        return await HasCoupleDataAccessAsync(userId, connectionId);
    }

    public async Task<Guid?> GetPartnerIdAsync(Guid userId)
    {
        try
        {
            var connection = await _coupleConnectionRepository.GetByUserIdAsync(userId);
            if (connection == null)
            {
                return null;
            }

            return connection.User1Id == userId ? connection.User2Id : connection.User1Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner ID for user {UserId}", userId);
            return null;
        }
    }
}
