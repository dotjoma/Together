# Offline Sync Implementation

## Overview

The offline sync system enables users to continue using core features when network connectivity is unavailable. Changes made offline are queued and automatically synchronized when the connection is restored.

## Architecture

### Components

1. **OfflineSyncManager** (`Together.Infrastructure/Services/OfflineSyncManager.cs`)
   - Manages offline operation queue
   - Handles data caching for offline viewing
   - Performs synchronization when online
   - Monitors network connectivity

2. **OfflineCacheDbContext** (`Together.Infrastructure/Data/OfflineCacheDbContext.cs`)
   - SQLite database for local caching
   - Stores pending operations and cached data
   - Separate from main PostgreSQL database

3. **OfflineSyncViewModel** (`Together/ViewModels/OfflineSyncViewModel.cs`)
   - Manages UI state for offline/sync indicators
   - Monitors connection status
   - Triggers automatic synchronization

4. **UI Controls**
   - `OfflineIndicator.xaml` - Shows offline status banner
   - `SyncStatusIndicator.xaml` - Shows sync progress

## Features

### Offline Capabilities

**Allowed Offline:**
- View cached posts (last 100)
- View cached journal entries
- View cached mood history (30 days)
- Create journal entries (queued)
- Log mood entries (queued)
- Create/update todo items (queued)

**Prevented Offline:**
- Create or edit posts (requires real-time validation)
- Send follow requests (requires immediate server response)
- Like posts or add comments (requires real-time updates)

### Data Caching

The system automatically caches data for offline viewing:

- **Posts**: Last 100 posts from feed
- **Journal Entries**: All entries for user's connection
- **Mood Entries**: Last 30 days of mood data
- **Cache Expiration**: 7 days

### Operation Queue

Operations performed offline are queued with:
- Operation type
- Payload data (JSON serialized)
- Timestamp
- Retry count (max 3 attempts)
- Error message (if failed)

### Synchronization

**Automatic Sync:**
- Triggered every 30 seconds when online
- Triggered immediately when connection restored
- Triggered after SignalR reconnection

**Manual Sync:**
- User can trigger via UI (future enhancement)

**Sync Process:**
1. Check network connectivity
2. Retrieve pending operations
3. Process operations in chronological order
4. Update sync progress UI
5. Remove successful operations
6. Retry failed operations (up to 3 times)
7. Update cache with latest data

## Integration

### ViewModel Integration

ViewModels that support offline functionality inject `IOfflineSyncManager`:

```csharp
public class JournalViewModel : ViewModelBase
{
    private readonly IOfflineSyncManager? _offlineSyncManager;

    public JournalViewModel(
        IJournalService journalService,
        IOfflineSyncManager? offlineSyncManager = null)
    {
        _offlineSyncManager = offlineSyncManager;
    }

    private async Task LoadEntriesAsync()
    {
        // Try online first, fall back to cache
        if (_offlineSyncManager != null && !await _offlineSyncManager.IsOnlineAsync())
        {
            var cachedEntries = await _offlineSyncManager.GetCachedJournalEntriesAsync(_connectionId);
            // Use cached data
        }
        else
        {
            var entries = await _journalService.GetJournalEntriesAsync(_connectionId);
            // Cache for offline use
            await _offlineSyncManager.CacheJournalEntriesAsync(_connectionId, entries);
        }
    }
}
```

### Dependency Injection

Register in `App.xaml.cs`:

```csharp
// SQLite for offline cache
var offlineCacheOptions = new DbContextOptionsBuilder<OfflineCacheDbContext>()
    .UseSqlite($"Data Source={Path.Combine(appDataPath, "offline_cache.db")}")
    .Options;

services.AddSingleton(offlineCacheOptions);
services.AddSingleton<OfflineCacheDbContext>();
services.AddSingleton<IOfflineSyncManager, OfflineSyncManager>();
services.AddSingleton<OfflineSyncViewModel>();
```

### UI Integration

Add indicators to main layout:

```xaml
<StackPanel>
    <!-- Offline indicator -->
    <controls:OfflineIndicator DataContext="{Binding OfflineSyncViewModel}"/>
    
    <!-- Sync status indicator -->
    <controls:SyncStatusIndicator DataContext="{Binding OfflineSyncViewModel}"/>
    
    <!-- Main content -->
    <ContentControl Content="{Binding CurrentView}"/>
</StackPanel>
```

## Network Connectivity Detection

The system uses multiple methods to detect connectivity:

1. **NetworkInterface.GetIsNetworkAvailable()** - Quick check for network adapters
2. **Ping to 8.8.8.8** - Verifies actual internet connectivity
3. **SignalR connection status** - Real-time connection monitoring

## Error Handling

### Retry Logic

Failed operations are retried up to 3 times with:
- Immediate retry on first failure
- Exponential backoff for subsequent retries
- Error message stored for debugging

### User Feedback

- Offline banner shows pending operation count
- Sync progress shows current operation
- Error messages displayed for failed syncs
- Success confirmation after sync completes

## Performance Considerations

### Cache Management

- Automatic cleanup of old cached data (7 days)
- Limit cached posts to 100 most recent
- Limit mood history to 30 days
- SQLite database optimized with indexes

### Sync Optimization

- Operations processed in batches
- Progress updates throttled to avoid UI lag
- Background thread for sync operations
- Cancellation support for long-running syncs

## Testing

### Manual Testing

1. **Offline Mode:**
   - Disable network adapter
   - Verify offline indicator appears
   - Create journal entry
   - Verify operation queued
   - Re-enable network
   - Verify automatic sync

2. **Cache Viewing:**
   - Load data while online
   - Disable network
   - Navigate to cached views
   - Verify data displays correctly

3. **Sync Recovery:**
   - Queue multiple operations offline
   - Enable network
   - Verify all operations sync
   - Check for duplicate data

### Unit Testing

Test coverage for:
- Network connectivity detection
- Operation queuing
- Cache management
- Sync process
- Error handling

## Future Enhancements

1. **Conflict Resolution:**
   - Handle concurrent edits
   - Merge strategies for conflicts
   - User notification of conflicts

2. **Selective Sync:**
   - User control over what to sync
   - Priority-based sync queue
   - Bandwidth-aware sync

3. **Enhanced Caching:**
   - Predictive caching
   - User-specific cache size limits
   - Cache compression

4. **Offline Analytics:**
   - Track offline usage patterns
   - Optimize cache strategy
   - Improve sync efficiency

## Troubleshooting

### Common Issues

**Sync not triggering:**
- Check network connectivity
- Verify SignalR connection
- Check pending operation count
- Review error logs

**Cached data not displaying:**
- Verify cache database exists
- Check cache expiration
- Ensure data was cached while online

**Operations not queuing:**
- Verify offline detection working
- Check SQLite database permissions
- Review operation serialization

### Debug Logging

Enable detailed logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Together.Infrastructure.Services.OfflineSyncManager": "Debug"
    }
  }
}
```

## Requirements Satisfied

This implementation satisfies the following requirements from the specification:

- **19.1**: Offline creation of journal entries, mood logs, and to-do items
- **19.2**: Automatic synchronization when connectivity restored
- **19.3**: Caching of recent posts and couple data
- **19.4**: Clear offline indicator in UI
- **19.5**: Prevention of real-time actions when offline
