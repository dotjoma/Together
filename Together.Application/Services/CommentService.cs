using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<CommentDto> AddCommentAsync(Guid userId, CreateCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Content), new[] { "Content cannot be empty" } }
            });

        if (dto.Content.Length > 300)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Content), new[] { "Content cannot exceed 300 characters" } }
            });

        var post = await _postRepository.GetByIdAsync(dto.PostId);
        if (post == null)
            throw new NotFoundException(nameof(Post), dto.PostId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException(nameof(User), userId);

        var comment = new Comment(dto.PostId, userId, dto.Content);
        await _commentRepository.AddAsync(comment);

        post.IncrementCommentCount();
        await _postRepository.UpdateAsync(post);

        // Create notification for post author (if not commenting on own post)
        if (post.AuthorId != userId)
        {
            var notification = new Notification(
                post.AuthorId,
                "comment",
                $"{user.Username} commented on your post",
                dto.PostId
            );
            await _notificationRepository.AddAsync(notification);
        }

        return new CommentDto(
            comment.Id,
            comment.PostId,
            new UserDto(user.Id, user.Username, user.Email.Value, user.ProfilePictureUrl, user.Bio),
            comment.Content!,
            comment.CreatedAt
        );
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(Guid postId, int skip = 0, int take = 20)
    {
        var comments = await _commentRepository.GetByPostIdAsync(postId, skip, take);

        return comments.Select(c => new CommentDto(
            c.Id,
            c.PostId,
            new UserDto(c.Author.Id, c.Author.Username, c.Author.Email.Value, c.Author.ProfilePictureUrl, c.Author.Bio),
            c.Content!,
            c.CreatedAt
        ));
    }

    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        return await _commentRepository.GetCountByPostIdAsync(postId);
    }
}
