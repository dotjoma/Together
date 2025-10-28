namespace Together.Domain.Entities;

public class Challenge
{
    public Guid Id { get; private set; }
    public Guid ConnectionId { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public int Points { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool CompletedByUser1 { get; private set; }
    public bool CompletedByUser2 { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public CoupleConnection Connection { get; private set; } = null!;

    private Challenge() { }

    public Challenge(Guid connectionId, string title, string description, string category, 
                     int points = 10, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        var validCategories = new[] { "communication", "fun", "appreciation", "learning" };
        if (!validCategories.Contains(category.ToLower()))
            throw new ArgumentException("Invalid category", nameof(category));

        Id = Guid.NewGuid();
        ConnectionId = connectionId;
        Title = title;
        Description = description;
        Category = category.ToLower();
        Points = points;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddHours(24);
        CompletedByUser1 = false;
        CompletedByUser2 = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkCompletedByUser(bool isUser1)
    {
        if (isUser1)
            CompletedByUser1 = true;
        else
            CompletedByUser2 = true;
    }

    public bool IsFullyCompleted()
    {
        return CompletedByUser1 && CompletedByUser2;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }
}
