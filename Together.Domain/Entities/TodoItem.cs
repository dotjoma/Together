namespace Together.Domain.Entities;

public class TodoItem
{
    public Guid Id { get; private set; }
    public Guid ConnectionId { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? DueDate { get; private set; }
    public bool Completed { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public List<string> Tags { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public CoupleConnection Connection { get; private set; } = null!;
    public User? AssignedUser { get; private set; }
    public User Creator { get; private set; } = null!;

    private TodoItem()
    {
        Tags = new List<string>();
    }

    public TodoItem(Guid connectionId, Guid createdBy, string title, string? description = null, 
                    Guid? assignedTo = null, DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Id = Guid.NewGuid();
        ConnectionId = connectionId;
        CreatedBy = createdBy;
        Title = title;
        Description = description;
        AssignedTo = assignedTo;
        DueDate = dueDate;
        Completed = false;
        Tags = new List<string>();
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsComplete()
    {
        Completed = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsIncomplete()
    {
        Completed = false;
        CompletedAt = null;
    }

    public void Update(string title, string? description, Guid? assignedTo, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        Description = description;
        AssignedTo = assignedTo;
        DueDate = dueDate;
    }

    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
            Tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    public bool IsOverdue()
    {
        return !Completed && DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
    }
}
