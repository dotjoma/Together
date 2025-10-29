namespace Together.Domain.Entities;

/// <summary>
/// Cached post for offline viewing
/// </summary>
public class CachedPost
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorProfilePictureUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public string ImageUrlsJson { get; set; } = "[]";
    public DateTime CachedAt { get; set; }
}
