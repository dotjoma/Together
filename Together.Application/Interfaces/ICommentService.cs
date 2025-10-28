using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(Guid userId, CreateCommentDto dto);
    Task<IEnumerable<CommentDto>> GetCommentsAsync(Guid postId, int skip = 0, int take = 20);
    Task<int> GetCommentCountAsync(Guid postId);
}
