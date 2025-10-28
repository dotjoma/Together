namespace Together.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; }
    public string Message { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    private Notification() { }

    public Notification(Guid userId, string type, string message, Guid? relatedEntityId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Message = message;
        RelatedEntityId = relatedEntityId;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
