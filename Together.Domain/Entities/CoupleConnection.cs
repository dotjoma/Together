using Together.Domain.Enums;

namespace Together.Domain.Entities;

public class CoupleConnection
{
    public Guid Id { get; private set; }
    public Guid User1Id { get; private set; }
    public Guid User2Id { get; private set; }
    public DateTime EstablishedAt { get; private set; }
    public DateTime RelationshipStartDate { get; private set; }
    public int LoveStreak { get; private set; }
    public DateTime? LastInteractionDate { get; private set; }
    public ConnectionStatus Status { get; private set; }
    public DateTime? NextMeetingDate { get; private set; }

    // Navigation properties
    public User User1 { get; private set; } = null!;
    public User User2 { get; private set; } = null!;
    public VirtualPet? VirtualPet { get; private set; }
    public ICollection<JournalEntry> JournalEntries { get; private set; }
    public ICollection<TodoItem> TodoItems { get; private set; }
    public ICollection<SharedEvent> Events { get; private set; }
    public ICollection<Challenge> Challenges { get; private set; }

    private CoupleConnection()
    {
        JournalEntries = new List<JournalEntry>();
        TodoItems = new List<TodoItem>();
        Events = new List<SharedEvent>();
        Challenges = new List<Challenge>();
    }

    public CoupleConnection(Guid user1Id, Guid user2Id, DateTime relationshipStartDate)
    {
        if (user1Id == user2Id)
            throw new ArgumentException("Users must be different");

        Id = Guid.NewGuid();
        User1Id = user1Id;
        User2Id = user2Id;
        EstablishedAt = DateTime.UtcNow;
        RelationshipStartDate = relationshipStartDate;
        LoveStreak = 0;
        Status = ConnectionStatus.Active;
        
        JournalEntries = new List<JournalEntry>();
        TodoItems = new List<TodoItem>();
        Events = new List<SharedEvent>();
        Challenges = new List<Challenge>();
    }

    public void IncrementStreak()
    {
        LoveStreak++;
        LastInteractionDate = DateTime.UtcNow;
    }

    public void ResetStreak()
    {
        LoveStreak = 0;
    }

    public void RecordInteraction()
    {
        LastInteractionDate = DateTime.UtcNow;
    }

    public void Terminate()
    {
        Status = ConnectionStatus.Terminated;
    }

    public void Archive()
    {
        Status = ConnectionStatus.Archived;
    }

    public void SetNextMeetingDate(DateTime? nextMeetingDate)
    {
        NextMeetingDate = nextMeetingDate;
    }
}
