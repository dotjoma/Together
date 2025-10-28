using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, int skip, int take);
    Task<int> GetCountByPostIdAsync(Guid postId);
    Task AddAsync(Comment comment);
    Task DeleteAsync(Comment comment);
}
