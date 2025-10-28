using Together.Domain.Enums;

namespace Together.Domain.Entities;

public class MoodEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public MoodType Mood { get; private set; }
    public string? Notes { get; private set; }
    public DateTime Timestamp { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private MoodEntry() { }

    public MoodEntry(Guid userId, MoodType mood, string? notes = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Mood = mood;
        Notes = notes;
        Timestamp = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
}
