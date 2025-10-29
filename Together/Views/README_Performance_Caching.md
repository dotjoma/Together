# Performance Optimization - Caching and Virtualization

## Overview
This document describes the performance optimizations implemented for caching and virtualization in the Together application.

## Implemented Features

### 1. In-Memory Caching Service

**Location**: `Together.Infrastructure/Services/MemoryCacheService.cs`

**Purpose**: Caches frequently accessed data in memory to reduce database queries and improve response times.

**Features**:
- Thread-safe concurrent dictionary implementation
- Automatic expiration handling (default 15 minutes)
- Generic type support
- Cache invalidation support

**Usage Example**:
```csharp
// In a service
private readonly IMemoryCacheService _cacheService;

public async Task<UserDto> GetUserAsync(Guid userId)
{
    var cacheKey = $"user_{userId}";
    return await _cacheService.GetOrCreateAsync(
        cacheKey,
        async () => await _userRepository.GetByIdAsync(userId),
        TimeSpan.FromMinutes(10)
    );
}
```

**Cache Keys Convention**:
- User session: `user_session_{userId}`
- User profile: `user_profile_{userId}`
- Couple connection: `couple_connection_{userId}`
- Feed cache: `feed_{userId}_{page}`

### 2. Image Lazy Loading and Caching

**Location**: `Together.Infrastructure/Services/ImageCacheService.cs`

**Purpose**: Lazy loads images and caches them in memory to avoid repeated downloads.

**Features**:
- Concurrent loading prevention (multiple requests for same image)
- Support for both local files and URLs
- Thread-safe bitmap creation with Freeze()
- Automatic cache size management (100 MB limit)
- Preloading support for anticipated images

**Usage Example**:
```csharp
// In a ViewModel
private readonly IImageCacheService _imageCacheService;

public async Task LoadProfilePictureAsync(string url)
{
    var bitmap = await _imageCacheService.LoadImageAsync(url);
    ProfilePicture = bitmap;
}

// Preload images for better UX
await _imageCacheService.PreloadImagesAsync(
    post.Image1Url,
    post.Image2Url,
    post.Image3Url
);
```

### 3. Debouncing for Search Inputs

**Location**: `Together/Utilities/DebouncedAction.cs`

**Purpose**: Prevents excessive API calls during user typing by delaying action execution.

**Features**:
- Configurable delay (default 300ms)
- Automatic cancellation of pending actions
- Support for both sync and async actions
- Thread-safe implementation

**Usage Example**:
```csharp
// In a ViewModel
private readonly DebouncedAction _searchDebouncer;

public string SearchQuery
{
    get => _searchQuery;
    set
    {
        SetProperty(ref _searchQuery, value);
        _searchDebouncer.Debounce(async () => await PerformSearchAsync());
    }
}

private async Task PerformSearchAsync()
{
    // This will only execute 300ms after user stops typing
    var results = await _searchService.SearchAsync(SearchQuery);
    SearchResults = results;
}
```

### 4. VirtualizingStackPanel Implementation

**Purpose**: Improves performance for long lists by only rendering visible items.

**Implemented In**:
- `SocialFeedView.xaml` - Posts list
- `JournalView.xaml` - Journal entries
- `TodoListView.xaml` - Todo items
- `FollowerListView.xaml` - Followers list
- `FollowingListView.xaml` - Following list
- `MoodHistoryView.xaml` - Mood history

**Configuration**:
```xml
<ItemsControl ItemsSource="{Binding Items}"
             VirtualizingPanel.IsVirtualizing="True"
             VirtualizingPanel.VirtualizationMode="Recycling">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <!-- ItemTemplate here -->
</ItemsControl>
```

**Benefits**:
- Only visible items are rendered
- Recycling mode reuses item containers
- Dramatically reduces memory usage for large lists
- Improves scrolling performance

## Service Registration

All performance services are registered in `App.xaml.cs`:

```csharp
// Performance Optimization Services
services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
services.AddSingleton<IImageCacheService, ImageCacheService>();
```

## Performance Metrics

### Expected Improvements:
- **Memory Usage**: 40-60% reduction for large lists with virtualization
- **Initial Load Time**: 30-50% faster with caching
- **Image Loading**: 80-90% faster for cached images
- **Search Responsiveness**: Eliminates unnecessary API calls during typing

### Cache Expiration Times:
- User session data: 15 minutes (default)
- User profiles: 10 minutes
- Feed data: 5 minutes
- Images: No expiration (cleared on app restart or manual clear)

## Best Practices

### When to Use Caching:
✅ Frequently accessed data (user profiles, session data)
✅ Data that changes infrequently
✅ Expensive database queries
✅ API responses with rate limits

### When NOT to Use Caching:
❌ Real-time data (mood updates, new posts)
❌ Sensitive data requiring fresh reads
❌ Data that changes frequently
❌ Large objects that consume too much memory

### Cache Invalidation:
Always invalidate cache when data changes:
```csharp
// After updating user profile
_cacheService.Remove($"user_profile_{userId}");

// After creating a post
_cacheService.Remove($"feed_{userId}_0");
```

## Monitoring

To monitor cache performance:
```csharp
// Check cache size
var cacheSize = _imageCacheService.GetCacheSize();
Log.Information("Image cache size: {Size} bytes", cacheSize);

// Clear cache if needed
if (cacheSize > MaxCacheSizeBytes)
{
    _imageCacheService.ClearCache();
}
```

## Future Enhancements

Potential improvements for future iterations:
1. Distributed caching with Redis for multi-instance scenarios
2. Cache warming on application startup
3. Intelligent cache eviction policies (LRU, LFU)
4. Cache hit/miss metrics and monitoring
5. Configurable cache sizes per data type
6. Background cache refresh for stale data
