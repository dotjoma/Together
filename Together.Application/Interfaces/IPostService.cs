using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(Guid authorId, CreatePostDto dto);
    Task<PostDto> UpdatePostAsync(Guid userId, UpdatePostDto dto);
    Task DeletePostAsync(Guid userId, Guid postId);
    Task<PostDto?> GetPostByIdAsync(Guid postId);
    Task<IEnumerable<PostDto>> GetUserPostsAsync(Guid userId, int skip = 0, int take = 20);
    Task<IEnumerable<PostDto>> GetFeedPostsAsync(Guid userId, int skip = 0, int take = 20);
}
