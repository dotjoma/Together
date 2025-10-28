namespace Together.Application.Interfaces;

public interface ILikeService
{
    Task<bool> ToggleLikeAsync(Guid postId, Guid userId);
    Task<bool> IsLikedByUserAsync(Guid postId, Guid userId);
}
