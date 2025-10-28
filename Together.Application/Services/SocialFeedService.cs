using Microsoft.Extensions.Caching.Memory;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class SocialFeedService : ISocialFeedService
{
    private readonly IPostRepository _postRepository;
    private readonly IFollowRelationshipRepository _followRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private const int PostsPerPage = 20;
    private const int CacheExpirationMinutes = 5;

    public SocialFeedService(
        IPostRepository postRepository,
        IFollowRelationshipRepository followRepository,
        IUserRepository userRepository,
        IMemoryCache cache)
    {
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<FeedResult> GetFeedAsync(Guid userId, int skip = 0, int take = 20)
    {
        // Validate pagination parameters
        if (skip < 0) skip = 0;
        if (take <= 0 || take > 100) take = PostsPerPage;

        // Try to get from cache for first page
        string cacheKey = $"feed_{userId}_{skip}_{take}";
        if (skip == 0 && _cache.TryGetValue(cacheKey, out FeedResult? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        // Get posts from followed users
        var posts = await _postRepository.GetFeedPostsAsync(userId, skip, take + 1);
        var postList = posts.ToList();

        // Check if there are more posts
        bool hasMore = postList.Count > take;
        if (hasMore)
        {
            postList = postList.Take(take).ToList();
        }

        // Map to DTOs
        var postDtos = postList.Select(p => MapToDto(p, p.Author)).ToList();

        // Get total count (approximate based on current page)
        int totalCount = skip + postDtos.Count + (hasMore ? 1 : 0);

        var result = new FeedResult(postDtos, totalCount, hasMore);

        // Cache first page only
        if (skip == 0)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));
            _cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }

    public async Task<IEnumerable<UserDto>> GetSuggestedUsersAsync(Guid userId, int limit = 5)
    {
        // Validate limit
        if (limit <= 0 || limit > 20) limit = 5;

        // Get users that the current user is already following
        var followingIds = await _followRepository.GetFollowingAsync(userId);
        var followingUserIds = followingIds.Select(f => f.FollowingId).ToHashSet();
        followingUserIds.Add(userId); // Exclude self

        // Get users followed by people the current user follows (mutual connections)
        var mutualConnectionIds = new HashSet<Guid>();
        foreach (var followingId in followingUserIds.Where(id => id != userId))
        {
            var theirFollowing = await _followRepository.GetFollowingAsync(followingId);
            foreach (var relationship in theirFollowing)
            {
                if (!followingUserIds.Contains(relationship.FollowingId))
                {
                    mutualConnectionIds.Add(relationship.FollowingId);
                }
            }
        }

        // Get suggested users (limit to requested amount)
        var suggestedUsers = new List<User>();
        foreach (var mutualId in mutualConnectionIds.Take(limit))
        {
            var user = await _userRepository.GetByIdAsync(mutualId);
            if (user != null && user.Visibility == Domain.Enums.ProfileVisibility.Public)
            {
                suggestedUsers.Add(user);
            }
        }

        // If we don't have enough suggestions from mutual connections, get random public users
        if (suggestedUsers.Count < limit)
        {
            var additionalUsers = await _userRepository.SearchUsersAsync("", limit * 2);
            foreach (var user in additionalUsers)
            {
                if (suggestedUsers.Count >= limit) break;
                
                if (!followingUserIds.Contains(user.Id) && 
                    !suggestedUsers.Any(u => u.Id == user.Id) &&
                    user.Visibility == Domain.Enums.ProfileVisibility.Public)
                {
                    suggestedUsers.Add(user);
                }
            }
        }

        return suggestedUsers.Take(limit).Select(MapUserToDto);
    }

    public async Task RefreshFeedCacheAsync(Guid userId)
    {
        // Invalidate cache for user's feed
        for (int skip = 0; skip < 100; skip += PostsPerPage)
        {
            string cacheKey = $"feed_{userId}_{skip}_{PostsPerPage}";
            _cache.Remove(cacheKey);
        }

        await Task.CompletedTask;
    }

    private PostDto MapToDto(Post post, User author)
    {
        var authorDto = new UserDto(
            author.Id,
            author.Username ?? string.Empty,
            author.Email?.Value ?? string.Empty,
            author.ProfilePictureUrl,
            author.Bio
        );

        var imageUrls = post.Images?
            .OrderBy(i => i.Order)
            .Select(i => i.ImageUrl ?? string.Empty)
            .ToList() ?? new List<string>();

        return new PostDto(
            post.Id,
            authorDto,
            post.Content ?? string.Empty,
            post.CreatedAt,
            post.EditedAt,
            post.LikeCount,
            post.CommentCount,
            imageUrls
        );
    }

    private UserDto MapUserToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username ?? string.Empty,
            user.Email?.Value ?? string.Empty,
            user.ProfilePictureUrl,
            user.Bio
        );
    }
}
