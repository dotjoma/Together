using Together.Domain.Enums;

namespace Together.Domain.Entities;

/// <summary>
/// Cached mood entry for offline viewing
/// </summary>
public class CachedMoodEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public MoodType Mood { get; set; }
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CachedAt { get; set; }
}
