using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;
    private readonly INotificationRepository _notificationRepository;

    public LikeService(
        ILikeRepository likeRepository,
        IPostRepository postRepository,
        INotificationRepository notificationRepository)
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> ToggleLikeAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new NotFoundException(nameof(Post), postId);

        var existingLike = await _likeRepository.GetByPostAndUserAsync(postId, userId);

        if (existingLike != null)
        {
            // Unlike
            await _likeRepository.DeleteAsync(existingLike);
            post.DecrementLikeCount();
            await _postRepository.UpdateAsync(post);
            return false; // Unliked
        }
        else
        {
            // Like
            var like = new Like(postId, userId);
            await _likeRepository.AddAsync(like);
            post.IncrementLikeCount();
            await _postRepository.UpdateAsync(post);

            // Create notification for post author (if not liking own post)
            if (post.AuthorId != userId)
            {
                var notification = new Notification(
                    post.AuthorId,
                    "like",
                    "Someone liked your post",
                    postId
                );
                await _notificationRepository.AddAsync(notification);
            }

            return true; // Liked
        }
    }

    public async Task<bool> IsLikedByUserAsync(Guid postId, Guid userId)
    {
        return await _likeRepository.ExistsAsync(postId, userId);
    }
}
