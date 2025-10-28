namespace Together.Domain.Entities;

public class FollowRelationship
{
    public Guid Id { get; private set; }
    public Guid FollowerId { get; private set; }
    public Guid FollowingId { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    // Navigation properties
    public User Follower { get; private set; } = null!;
    public User Following { get; private set; } = null!;

    private FollowRelationship() 
    {
        Status = "pending";
    }

    public FollowRelationship(Guid followerId, Guid followingId)
    {
        if (followerId == followingId)
            throw new ArgumentException("Users must be different");

        Id = Guid.NewGuid();
        FollowerId = followerId;
        FollowingId = followingId;
        Status = "pending";
        CreatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        Status = "accepted";
        AcceptedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = "rejected";
    }

    public bool IsPending()
    {
        return Status == "pending";
    }

    public bool IsAccepted()
    {
        return Status == "accepted";
    }
}
