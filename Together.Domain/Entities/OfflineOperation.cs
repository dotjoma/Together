using Together.Domain.Enums;

namespace Together.Domain.Entities;

/// <summary>
/// Represents an operation queued for synchronization when offline
/// </summary>
public class OfflineOperation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OperationType OperationType { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
