using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id);
    Task<IEnumerable<Post>> GetUserPostsAsync(Guid userId, int skip, int take);
    Task<IEnumerable<Post>> GetFeedPostsAsync(Guid userId, int skip, int take);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
}
