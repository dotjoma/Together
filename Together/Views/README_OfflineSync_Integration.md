# Offline Sync Integration Checklist

## Prerequisites

✅ All offline sync files created
✅ SQLite package added to Infrastructure project
✅ No compilation errors

## Integration Steps

### Step 1: Update App.xaml.cs

Add offline sync services to dependency injection:

```csharp
// In ConfigureServices method

// Get app data path for SQLite database
var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Together"
);

// Ensure directory exists
Directory.CreateDirectory(appDataPath);

// Configure SQLite for offline cache
var offlineCacheOptions = new DbContextOptionsBuilder<OfflineCacheDbContext>()
    .UseSqlite($"Data Source={Path.Combine(appDataPath, "offline_cache.db")}")
    .Options;

// Register offline sync services
services.AddSingleton(offlineCacheOptions);
services.AddSingleton<OfflineCacheDbContext>();
services.AddSingleton<IOfflineSyncManager, OfflineSyncManager>();
services.AddSingleton<OfflineSyncViewModel>();
```

### Step 2: Update ViewModel Registrations

Update existing ViewModel registrations to include `IOfflineSyncManager`:

```csharp
// Journal ViewModel
services.AddTransient<JournalViewModel>(sp => new JournalViewModel(
    sp.GetRequiredService<IJournalService>(),
    currentUserId,
    connectionId,
    sp.GetService<IRealTimeSyncService>(),
    sp.GetService<IOfflineSyncManager>()  // Add this
));

// Mood Selector ViewModel
services.AddTransient<MoodSelectorViewModel>(sp => new MoodSelectorViewModel(
    sp.GetRequiredService<IMoodTrackingService>(),
    currentUserId,
    sp.GetService<IOfflineSyncManager>()  // Add this
));

// Post Creation ViewModel
services.AddTransient<PostCreationViewModel>(sp => new PostCreationViewModel(
    sp.GetRequiredService<IPostService>(),
    currentUserId,
    sp.GetService<IOfflineSyncManager>()  // Add this
));

// Social Feed ViewModel
services.AddTransient<SocialFeedViewModel>(sp => new SocialFeedViewModel(
    sp.GetRequiredService<ISocialFeedService>(),
    sp.GetRequiredService<IPostService>(),
    sp.GetRequiredService<ILikeService>(),
    sp.GetRequiredService<ICommentService>(),
    currentUserId,
    sp.GetService<IRealTimeSyncService>(),
    sp.GetService<IOfflineSyncManager>()  // Add this
));
```

### Step 3: Update MainWindow.xaml

Add offline and sync indicators to the main layout:

```xaml
<Window x:Class="Together.MainWindow"
        xmlns:controls="clr-namespace:Together.Controls">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Offline indicator -->
            <RowDefinition Height="Auto"/> <!-- Sync status -->
            <RowDefinition Height="*"/>    <!-- Main content -->
        </Grid.RowDefinitions>
        
        <!-- Offline Indicator -->
        <controls:OfflineIndicator Grid.Row="0"
                                  DataContext="{Binding OfflineSyncViewModel}"/>
        
        <!-- Sync Status Indicator -->
        <controls:SyncStatusIndicator Grid.Row="1"
                                     DataContext="{Binding OfflineSyncViewModel}"/>
        
        <!-- Main Content -->
        <ContentControl Grid.Row="2"
                       Content="{Binding CurrentView}"/>
    </Grid>
</Window>
```

### Step 4: Update MainViewModel

Add `OfflineSyncViewModel` property to MainViewModel:

```csharp
public class MainViewModel : ViewModelBase
{
    public OfflineSyncViewModel OfflineSyncViewModel { get; }
    
    public MainViewModel(
        // ... other dependencies
        OfflineSyncViewModel offlineSyncViewModel)
    {
        OfflineSyncViewModel = offlineSyncViewModel;
        // ... rest of constructor
    }
}
```

### Step 5: Initialize Current User in OfflineSyncManager

After user login, set the current user ID:

```csharp
// In LoginViewModel or wherever login succeeds
var offlineSyncManager = serviceProvider.GetService<IOfflineSyncManager>();
if (offlineSyncManager is OfflineSyncManager manager)
{
    manager.SetCurrentUser(userId);
}
```

### Step 6: Complete Service Integration in OfflineSyncManager

Update the `ProcessOperationAsync` method to inject and use actual services:

```csharp
// In OfflineSyncManager constructor, add service dependencies:
private readonly IJournalService _journalService;
private readonly IMoodTrackingService _moodService;
private readonly ITodoService _todoService;

public OfflineSyncManager(
    OfflineCacheDbContext cacheContext,
    IAuthenticationService authService,
    IJournalService journalService,
    IMoodTrackingService moodService,
    ITodoService todoService,
    ILogger<OfflineSyncManager>? logger = null)
{
    _cacheContext = cacheContext;
    _authService = authService;
    _journalService = journalService;
    _moodService = moodService;
    _todoService = todoService;
    _logger = logger;
    
    _cacheContext.Database.EnsureCreated();
}

// Then update ProcessOperationAsync:
private async Task ProcessOperationAsync(OfflineOperation operation)
{
    switch (operation.OperationType)
    {
        case OperationType.CreateJournalEntry:
            var journalDto = JsonSerializer.Deserialize<CreateJournalEntryDto>(operation.PayloadJson);
            await _journalService.CreateEntryAsync(journalDto);
            break;

        case OperationType.CreateMoodEntry:
            var moodDto = JsonSerializer.Deserialize<CreateMoodEntryDto>(operation.PayloadJson);
            await _moodService.CreateMoodEntryAsync(operation.UserId, moodDto);
            break;

        case OperationType.CreateTodoItem:
            var todoDto = JsonSerializer.Deserialize<CreateTodoItemDto>(operation.PayloadJson);
            await _todoService.CreateTodoItemAsync(todoDto);
            break;

        case OperationType.UpdateTodoItem:
            var updateDto = JsonSerializer.Deserialize<UpdateTodoItemDto>(operation.PayloadJson);
            await _todoService.UpdateTodoItemAsync(updateDto);
            break;

        case OperationType.CompleteTodoItem:
            var todoId = JsonSerializer.Deserialize<Guid>(operation.PayloadJson);
            await _todoService.MarkAsCompleteAsync(todoId);
            break;

        default:
            throw new InvalidOperationException($"Unknown operation type: {operation.OperationType}");
    }
}
```

### Step 7: Add Required DTOs

Ensure these DTOs exist (create if missing):

- `CreateJournalEntryDto`
- `CreateMoodEntryDto` ✅ (already exists)
- `CreateTodoItemDto`
- `UpdateTodoItemDto`

### Step 8: Test Integration

1. **Build Solution:**
   ```bash
   dotnet build Together.sln
   ```

2. **Run Application:**
   ```bash
   dotnet run --project Together/Together.csproj
   ```

3. **Test Offline Mode:**
   - Disable network adapter
   - Verify offline indicator appears
   - Create journal entry
   - Log mood
   - Re-enable network
   - Verify automatic sync

4. **Test Cache:**
   - Load data while online
   - Disable network
   - Navigate to different views
   - Verify cached data displays

## Verification Checklist

- [ ] SQLite database created in AppData folder
- [ ] Offline indicator appears when network disabled
- [ ] Pending operation count displays correctly
- [ ] Sync status indicator shows during sync
- [ ] Journal entries queue when offline
- [ ] Mood entries queue when offline
- [ ] Post creation prevented when offline
- [ ] Cached posts display when offline
- [ ] Automatic sync triggers when online
- [ ] Sync progress updates correctly
- [ ] Failed operations retry up to 3 times
- [ ] Old cache data cleaned up after 7 days

## Troubleshooting

### Issue: SQLite database not created

**Solution:** Ensure app data directory exists and has write permissions:
```csharp
var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Together"
);
Directory.CreateDirectory(appDataPath);
```

### Issue: Offline indicator not showing

**Solution:** Verify `OfflineSyncViewModel` is properly bound in MainWindow:
```xaml
<controls:OfflineIndicator DataContext="{Binding OfflineSyncViewModel}"/>
```

### Issue: Operations not syncing

**Solution:** Check that services are properly injected in `OfflineSyncManager` and `ProcessOperationAsync` is implemented.

### Issue: Cached data not displaying

**Solution:** Verify caching is called after loading data:
```csharp
var entries = await _journalService.GetJournalEntriesAsync(_connectionId);
await _offlineSyncManager.CacheJournalEntriesAsync(_connectionId, entries.Cast<object>());
```

## Performance Monitoring

Monitor these metrics after integration:

- SQLite database size
- Sync operation duration
- Network connectivity check latency
- Cache hit rate
- Failed operation count

## Security Considerations

- SQLite database stored in user's AppData (not shared)
- No sensitive data in operation queue (use IDs, not passwords)
- Cache automatically expires after 7 days
- Network connectivity checks don't expose user data

## Next Steps After Integration

1. Add user settings for offline behavior
2. Implement manual sync button
3. Add sync history view
4. Implement conflict resolution UI
5. Add offline analytics dashboard

## Support

For issues or questions about offline sync integration:
- Review `README_OfflineSync.md` for detailed documentation
- Check `README_OfflineSync_Summary.md` for implementation overview
- Enable debug logging for `OfflineSyncManager`
