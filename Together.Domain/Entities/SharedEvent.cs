namespace Together.Domain.Entities;

public class SharedEvent
{
    public Guid Id { get; private set; }
    public Guid ConnectionId { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime EventDate { get; private set; }
    public string? Recurrence { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public CoupleConnection Connection { get; private set; } = null!;
    public User Creator { get; private set; } = null!;

    private SharedEvent() 
    {
        Recurrence = "none";
    }

    public SharedEvent(Guid connectionId, Guid createdBy, string title, DateTime eventDate, 
                       string? description = null, string recurrence = "none")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        var validRecurrences = new[] { "none", "daily", "weekly", "monthly", "yearly" };
        if (!validRecurrences.Contains(recurrence.ToLower()))
            throw new ArgumentException("Invalid recurrence type", nameof(recurrence));

        Id = Guid.NewGuid();
        ConnectionId = connectionId;
        CreatedBy = createdBy;
        Title = title;
        EventDate = eventDate;
        Description = description;
        Recurrence = recurrence.ToLower();
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, DateTime eventDate, string? description, string recurrence)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        var validRecurrences = new[] { "none", "daily", "weekly", "monthly", "yearly" };
        if (!validRecurrences.Contains(recurrence.ToLower()))
            throw new ArgumentException("Invalid recurrence type", nameof(recurrence));

        Title = title;
        EventDate = eventDate;
        Description = description;
        Recurrence = recurrence.ToLower();
    }
}
