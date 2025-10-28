namespace Together.Domain.Entities;

public class ConnectionRequest
{
    public Guid Id { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public ConnectionRequestStatus Status { get; private set; }

    // Navigation properties
    public User FromUser { get; private set; } = null!;
    public User ToUser { get; private set; } = null!;

    private ConnectionRequest()
    {
    }

    public ConnectionRequest(Guid fromUserId, Guid toUserId)
    {
        if (fromUserId == toUserId)
            throw new ArgumentException("Cannot send connection request to yourself");

        Id = Guid.NewGuid();
        FromUserId = fromUserId;
        ToUserId = toUserId;
        CreatedAt = DateTime.UtcNow;
        Status = ConnectionRequestStatus.Pending;
    }

    public void Accept()
    {
        if (Status != ConnectionRequestStatus.Pending)
            throw new InvalidOperationException("Can only accept pending requests");

        Status = ConnectionRequestStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != ConnectionRequestStatus.Pending)
            throw new InvalidOperationException("Can only reject pending requests");

        Status = ConnectionRequestStatus.Rejected;
        RespondedAt = DateTime.UtcNow;
    }
}

public enum ConnectionRequestStatus
{
    Pending,
    Accepted,
    Rejected
}
