using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Text.Json;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Services;

/// <summary>
/// Manages offline data caching and synchronization
/// </summary>
public class OfflineSyncManager : IOfflineSyncManager
{
    private readonly OfflineCacheDbContext _cacheContext;
    private readonly ILogger<OfflineSyncManager>? _logger;
    private readonly IAuthenticationService _authService;
    private Guid _currentUserId;
    private const int MaxRetryCount = 3;
    private const int CacheExpirationDays = 7;

    public event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

    public OfflineSyncManager(
        OfflineCacheDbContext cacheContext,
        IAuthenticationService authService,
        ILogger<OfflineSyncManager>? logger = null)
    {
        _cacheContext = cacheContext;
        _authService = authService;
        _logger = logger;

        // Ensure database is created
        _cacheContext.Database.EnsureCreated();
    }

    public void SetCurrentUser(Guid userId)
    {
        _currentUserId = userId;
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            // Check network interface status
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            // Try to ping a reliable host
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error checking online status");
            return false;
        }
    }

    public async Task QueueOperationAsync(OperationType operationType, object payload)
    {
        try
        {
            var operation = new OfflineOperation
            {
                Id = Guid.NewGuid(),
                UserId = _currentUserId,
                OperationType = operationType,
                PayloadJson = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            _cacheContext.OfflineOperations.Add(operation);
            await _cacheContext.SaveChangesAsync();

            _logger?.LogInformation("Queued offline operation: {OperationType}", operationType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to queue offline operation");
            throw;
        }
    }

    public async Task<SyncResult> SyncPendingOperationsAsync()
    {
        var result = new SyncResult { Success = true };

        try
        {
            // Check if online
            if (!await IsOnlineAsync())
            {
                _logger?.LogWarning("Cannot sync: offline");
                result.Success = false;
                result.Errors.Add("Device is offline");
                return result;
            }

            // Get pending operations
            var operations = await _cacheContext.OfflineOperations
                .Where(o => o.UserId == _currentUserId && o.RetryCount < MaxRetryCount)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            if (operations.Count == 0)
            {
                return result;
            }

            _logger?.LogInformation("Syncing {Count} pending operations", operations.Count);

            // Raise sync started event
            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
            {
                IsSyncing = true,
                TotalOperations = operations.Count,
                CompletedOperations = 0
            });

            int completed = 0;

            foreach (var operation in operations)
            {
                try
                {
                    // Update progress
                    SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
                    {
                        IsSyncing = true,
                        TotalOperations = operations.Count,
                        CompletedOperations = completed,
                        CurrentOperation = operation.OperationType.ToString()
                    });

                    // Process the operation based on type
                    await ProcessOperationAsync(operation);

                    // Remove successful operation
                    _cacheContext.OfflineOperations.Remove(operation);
                    result.SuccessfulOperations++;
                    completed++;

                    _logger?.LogDebug("Successfully synced operation: {OperationType}", operation.OperationType);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to sync operation: {OperationType}", operation.OperationType);

                    // Increment retry count
                    operation.RetryCount++;
                    operation.ErrorMessage = ex.Message;

                    if (operation.RetryCount >= MaxRetryCount)
                    {
                        _logger?.LogWarning("Operation exceeded max retries, removing: {OperationType}", operation.OperationType);
                        _cacheContext.OfflineOperations.Remove(operation);
                    }

                    result.FailedOperations++;
                    result.Errors.Add($"{operation.OperationType}: {ex.Message}");
                }
            }

            await _cacheContext.SaveChangesAsync();

            // Raise sync completed event
            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
            {
                IsSyncing = false,
                TotalOperations = operations.Count,
                CompletedOperations = completed
            });

            _logger?.LogInformation("Sync completed: {Success} successful, {Failed} failed",
                result.SuccessfulOperations, result.FailedOperations);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during sync");
            result.Success = false;
            result.Errors.Add(ex.Message);

            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs
            {
                IsSyncing = false
            });
        }

        return result;
    }

    private async Task ProcessOperationAsync(OfflineOperation operation)
    {
        // Note: In a real implementation, you would inject the appropriate services
        // and call their methods. For now, we'll throw NotImplementedException
        // to indicate where service calls should be made.

        switch (operation.OperationType)
        {
            case OperationType.CreateJournalEntry:
                // var dto = JsonSerializer.Deserialize<CreateJournalEntryDto>(operation.PayloadJson);
                // await _journalService.CreateEntryAsync(dto);
                throw new NotImplementedException("Journal service integration required");

            case OperationType.CreateMoodEntry:
                // var dto = JsonSerializer.Deserialize<CreateMoodEntryDto>(operation.PayloadJson);
                // await _moodService.CreateMoodEntryAsync(dto);
                throw new NotImplementedException("Mood service integration required");

            case OperationType.CreateTodoItem:
                // var dto = JsonSerializer.Deserialize<CreateTodoItemDto>(operation.PayloadJson);
                // await _todoService.CreateTodoItemAsync(dto);
                throw new NotImplementedException("Todo service integration required");

            case OperationType.UpdateTodoItem:
                // var dto = JsonSerializer.Deserialize<UpdateTodoItemDto>(operation.PayloadJson);
                // await _todoService.UpdateTodoItemAsync(dto);
                throw new NotImplementedException("Todo service integration required");

            case OperationType.CompleteTodoItem:
                // var todoId = JsonSerializer.Deserialize<Guid>(operation.PayloadJson);
                // await _todoService.MarkAsCompleteAsync(todoId);
                throw new NotImplementedException("Todo service integration required");

            default:
                throw new InvalidOperationException($"Unknown operation type: {operation.OperationType}");
        }
    }

    public async Task CachePostsAsync(IEnumerable<object> posts)
    {
        try
        {
            foreach (var postObj in posts)
            {
                if (postObj is not PostDto post) continue;

                var cachedPost = new CachedPost
                {
                    Id = post.Id,
                    AuthorId = post.Author.Id,
                    AuthorUsername = post.Author.Username,
                    AuthorProfilePictureUrl = post.Author.ProfilePictureUrl ?? string.Empty,
                    Content = post.Content,
                    CreatedAt = post.CreatedAt,
                    LikeCount = post.LikeCount,
                    CommentCount = post.CommentCount,
                    ImageUrlsJson = JsonSerializer.Serialize(post.ImageUrls),
                    CachedAt = DateTime.UtcNow
                };

                // Update or insert
                var existing = await _cacheContext.CachedPosts.FindAsync(post.Id);
                if (existing != null)
                {
                    _cacheContext.Entry(existing).CurrentValues.SetValues(cachedPost);
                }
                else
                {
                    _cacheContext.CachedPosts.Add(cachedPost);
                }
            }

            await _cacheContext.SaveChangesAsync();

            // Keep only the most recent 100 posts
            var oldPosts = await _cacheContext.CachedPosts
                .OrderByDescending(p => p.CachedAt)
                .Skip(100)
                .ToListAsync();

            if (oldPosts.Any())
            {
                _cacheContext.CachedPosts.RemoveRange(oldPosts);
                await _cacheContext.SaveChangesAsync();
            }

            _logger?.LogDebug("Cached {Count} posts", posts.Count());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to cache posts");
        }
    }

    public async Task<IEnumerable<object>> GetCachedPostsAsync(int limit = 100)
    {
        try
        {
            var cachedPosts = await _cacheContext.CachedPosts
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return cachedPosts.Select(cp => new PostDto(
                cp.Id,
                new UserDto(cp.AuthorId, cp.AuthorUsername, string.Empty, cp.AuthorProfilePictureUrl, string.Empty),
                cp.Content,
                cp.CreatedAt,
                null, // EditedAt
                cp.LikeCount,
                cp.CommentCount,
                JsonSerializer.Deserialize<List<string>>(cp.ImageUrlsJson) ?? new List<string>()
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get cached posts");
            return Enumerable.Empty<object>();
        }
    }

    public async Task CacheJournalEntriesAsync(Guid connectionId, IEnumerable<object> entries)
    {
        try
        {
            foreach (var entryObj in entries)
            {
                if (entryObj is not JournalEntryDto entry) continue;

                var cachedEntry = new CachedJournalEntry
                {
                    Id = entry.Id,
                    ConnectionId = connectionId,
                    AuthorId = entry.Author.Id,
                    AuthorUsername = entry.Author.Username,
                    Content = entry.Content,
                    CreatedAt = entry.CreatedAt,
                    IsReadByPartner = entry.IsReadByPartner,
                    ImageUrl = entry.ImageUrl,
                    CachedAt = DateTime.UtcNow
                };

                var existing = await _cacheContext.CachedJournalEntries.FindAsync(entry.Id);
                if (existing != null)
                {
                    _cacheContext.Entry(existing).CurrentValues.SetValues(cachedEntry);
                }
                else
                {
                    _cacheContext.CachedJournalEntries.Add(cachedEntry);
                }
            }

            await _cacheContext.SaveChangesAsync();
            _logger?.LogDebug("Cached {Count} journal entries", entries.Count());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to cache journal entries");
        }
    }

    public async Task<IEnumerable<object>> GetCachedJournalEntriesAsync(Guid connectionId)
    {
        try
        {
            var cachedEntries = await _cacheContext.CachedJournalEntries
                .Where(e => e.ConnectionId == connectionId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return cachedEntries.Select(ce => new JournalEntryDto(
                ce.Id,
                ce.ConnectionId,
                new UserDto(ce.AuthorId, ce.AuthorUsername, string.Empty, string.Empty, string.Empty),
                ce.Content,
                ce.CreatedAt,
                ce.IsReadByPartner,
                ce.ImageUrl
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get cached journal entries");
            return Enumerable.Empty<object>();
        }
    }

    public async Task CacheMoodEntriesAsync(Guid userId, IEnumerable<object> entries)
    {
        try
        {
            foreach (var entryObj in entries)
            {
                if (entryObj is not MoodEntryDto entry) continue;

                var cachedEntry = new CachedMoodEntry
                {
                    Id = entry.Id,
                    UserId = entry.UserId,
                    Mood = Enum.Parse<MoodType>(entry.Mood),
                    Notes = entry.Notes,
                    Timestamp = entry.Timestamp,
                    CachedAt = DateTime.UtcNow
                };

                var existing = await _cacheContext.CachedMoodEntries.FindAsync(entry.Id);
                if (existing != null)
                {
                    _cacheContext.Entry(existing).CurrentValues.SetValues(cachedEntry);
                }
                else
                {
                    _cacheContext.CachedMoodEntries.Add(cachedEntry);
                }
            }

            await _cacheContext.SaveChangesAsync();
            _logger?.LogDebug("Cached {Count} mood entries", entries.Count());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to cache mood entries");
        }
    }

    public async Task<IEnumerable<object>> GetCachedMoodEntriesAsync(Guid userId, int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var cachedEntries = await _cacheContext.CachedMoodEntries
                .Where(e => e.UserId == userId && e.Timestamp >= cutoffDate)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();

            return cachedEntries.Select(ce => new MoodEntryDto(
                ce.Id,
                ce.UserId,
                ce.Mood.ToString(),
                ce.Notes,
                ce.Timestamp
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get cached mood entries");
            return Enumerable.Empty<object>();
        }
    }

    public async Task InvalidateOldCacheAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-CacheExpirationDays);

            // Remove old cached posts
            var oldPosts = await _cacheContext.CachedPosts
                .Where(p => p.CachedAt < cutoffDate)
                .ToListAsync();

            if (oldPosts.Any())
            {
                _cacheContext.CachedPosts.RemoveRange(oldPosts);
                _logger?.LogInformation("Removed {Count} old cached posts", oldPosts.Count);
            }

            // Remove old cached journal entries
            var oldJournalEntries = await _cacheContext.CachedJournalEntries
                .Where(e => e.CachedAt < cutoffDate)
                .ToListAsync();

            if (oldJournalEntries.Any())
            {
                _cacheContext.CachedJournalEntries.RemoveRange(oldJournalEntries);
                _logger?.LogInformation("Removed {Count} old cached journal entries", oldJournalEntries.Count);
            }

            // Remove old cached mood entries
            var oldMoodEntries = await _cacheContext.CachedMoodEntries
                .Where(e => e.CachedAt < cutoffDate)
                .ToListAsync();

            if (oldMoodEntries.Any())
            {
                _cacheContext.CachedMoodEntries.RemoveRange(oldMoodEntries);
                _logger?.LogInformation("Removed {Count} old cached mood entries", oldMoodEntries.Count);
            }

            await _cacheContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to invalidate old cache");
        }
    }

    public async Task<int> GetPendingOperationCountAsync()
    {
        try
        {
            return await _cacheContext.OfflineOperations
                .Where(o => o.UserId == _currentUserId && o.RetryCount < MaxRetryCount)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get pending operation count");
            return 0;
        }
    }
}
