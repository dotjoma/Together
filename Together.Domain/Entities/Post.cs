namespace Together.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string? Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public int LikeCount { get; private set; }
    public int CommentCount { get; private set; }

    // Navigation properties
    public User Author { get; private set; } = null!;
    public ICollection<PostImage> Images { get; private set; }
    public ICollection<Like> Likes { get; private set; }
    public ICollection<Comment> Comments { get; private set; }

    private Post()
    {
        Images = new List<PostImage>();
        Likes = new List<Like>();
        Comments = new List<Comment>();
    }

    public Post(Guid authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));
        if (content.Length > 500)
            throw new ArgumentException("Content cannot exceed 500 characters", nameof(content));

        Id = Guid.NewGuid();
        AuthorId = authorId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        LikeCount = 0;
        CommentCount = 0;
        
        Images = new List<PostImage>();
        Likes = new List<Like>();
        Comments = new List<Comment>();
    }

    public void Edit(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty", nameof(newContent));
        if (newContent.Length > 500)
            throw new ArgumentException("Content cannot exceed 500 characters", nameof(newContent));

        var timeSinceCreation = DateTime.UtcNow - CreatedAt;
        if (timeSinceCreation.TotalMinutes > 15)
            throw new InvalidOperationException("Posts can only be edited within 15 minutes of creation");

        Content = newContent;
        EditedAt = DateTime.UtcNow;
    }

    public void IncrementLikeCount()
    {
        LikeCount++;
    }

    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
            LikeCount--;
    }

    public void IncrementCommentCount()
    {
        CommentCount++;
    }

    public void DecrementCommentCount()
    {
        if (CommentCount > 0)
            CommentCount--;
    }
}
