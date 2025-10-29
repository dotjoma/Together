using Together.Domain.Enums;

namespace Together.Application.Interfaces;

/// <summary>
/// Manages offline data caching and synchronization
/// </summary>
public interface IOfflineSyncManager
{
    /// <summary>
    /// Checks if the application is currently online
    /// </summary>
    Task<bool> IsOnlineAsync();

    /// <summary>
    /// Queues an operation for later synchronization when offline
    /// </summary>
    Task QueueOperationAsync(OperationType operationType, object payload);

    /// <summary>
    /// Synchronizes all pending operations with the server
    /// </summary>
    Task<SyncResult> SyncPendingOperationsAsync();

    /// <summary>
    /// Caches posts for offline viewing
    /// </summary>
    Task CachePostsAsync(IEnumerable<object> posts);

    /// <summary>
    /// Gets cached posts for offline viewing
    /// </summary>
    Task<IEnumerable<object>> GetCachedPostsAsync(int limit = 100);

    /// <summary>
    /// Caches journal entries for offline viewing
    /// </summary>
    Task CacheJournalEntriesAsync(Guid connectionId, IEnumerable<object> entries);

    /// <summary>
    /// Gets cached journal entries for offline viewing
    /// </summary>
    Task<IEnumerable<object>> GetCachedJournalEntriesAsync(Guid connectionId);

    /// <summary>
    /// Caches mood entries for offline viewing
    /// </summary>
    Task CacheMoodEntriesAsync(Guid userId, IEnumerable<object> entries);

    /// <summary>
    /// Gets cached mood entries for offline viewing
    /// </summary>
    Task<IEnumerable<object>> GetCachedMoodEntriesAsync(Guid userId, int days = 30);

    /// <summary>
    /// Invalidates old cached data
    /// </summary>
    Task InvalidateOldCacheAsync();

    /// <summary>
    /// Gets the count of pending operations
    /// </summary>
    Task<int> GetPendingOperationCountAsync();

    /// <summary>
    /// Event raised when sync status changes
    /// </summary>
    event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;
}

/// <summary>
/// Result of a synchronization operation
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Event args for sync status changes
/// </summary>
public class SyncStatusChangedEventArgs : EventArgs
{
    public bool IsSyncing { get; set; }
    public int TotalOperations { get; set; }
    public int CompletedOperations { get; set; }
    public string? CurrentOperation { get; set; }
}
