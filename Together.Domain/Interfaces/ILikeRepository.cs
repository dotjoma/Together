using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface ILikeRepository
{
    Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId);
    Task AddAsync(Like like);
    Task DeleteAsync(Like like);
    Task<bool> ExistsAsync(Guid postId, Guid userId);
}
