namespace Together.Domain.Entities;

public class Comment
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string? Content { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Post Post { get; private set; } = null!;
    public User Author { get; private set; } = null!;

    private Comment() { }

    public Comment(Guid postId, Guid authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));
        if (content.Length > 300)
            throw new ArgumentException("Content cannot exceed 300 characters", nameof(content));

        Id = Guid.NewGuid();
        PostId = postId;
        AuthorId = authorId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }
}
