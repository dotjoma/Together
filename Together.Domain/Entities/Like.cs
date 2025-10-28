namespace Together.Domain.Entities;

public class Like
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Post Post { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private Like() { }

    public Like(Guid postId, Guid userId)
    {
        Id = Guid.NewGuid();
        PostId = postId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
