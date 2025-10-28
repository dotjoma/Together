using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface ISocialFeedService
{
    Task<FeedResult> GetFeedAsync(Guid userId, int skip = 0, int take = 20);
    Task<IEnumerable<UserDto>> GetSuggestedUsersAsync(Guid userId, int limit = 5);
    Task RefreshFeedCacheAsync(Guid userId);
}
