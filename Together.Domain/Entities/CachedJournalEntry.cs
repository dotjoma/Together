namespace Together.Domain.Entities;

/// <summary>
/// Cached journal entry for offline viewing
/// </summary>
public class CachedJournalEntry
{
    public Guid Id { get; set; }
    public Guid ConnectionId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsReadByPartner { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CachedAt { get; set; }
}
