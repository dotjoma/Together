namespace Together.Domain.Entities;

public class JournalEntry
{
    public Guid Id { get; private set; }
    public Guid ConnectionId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string? Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsReadByPartner { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation properties
    public CoupleConnection Connection { get; private set; } = null!;
    public User Author { get; private set; } = null!;

    private JournalEntry() { }

    public JournalEntry(Guid connectionId, Guid authorId, string content, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        Id = Guid.NewGuid();
        ConnectionId = connectionId;
        AuthorId = authorId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        IsReadByPartner = false;
        ImageUrl = imageUrl;
    }

    public void MarkAsRead()
    {
        IsReadByPartner = true;
    }
}
