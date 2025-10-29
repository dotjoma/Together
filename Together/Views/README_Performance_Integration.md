# Performance Optimization Integration Guide

## Quick Start

This guide shows how to integrate the performance optimizations into your ViewModels and Services.

## 1. Using Memory Cache Service

### In a Service

```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCacheService _cacheService;

    public UserService(IUserRepository userRepository, IMemoryCacheService cacheService)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var cacheKey = $"user_{userId}";
        
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => 
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return MapToDto(user);
            },
            TimeSpan.FromMinutes(10) // Cache for 10 minutes
        );
    }

    public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        // Update user
        await _userRepository.UpdateAsync(user);
        
        // Invalidate cache
        _cacheService.Remove($"user_{userId}");
    }
}
```

### Cache Key Conventions

Use consistent naming for cache keys:
- User data: `user_{userId}`
- User profile: `user_profile_{userId}`
- Couple connection: `couple_connection_{userId}`
- Feed data: `feed_{userId}_{page}`
- User session: `user_session_{userId}`

## 2. Using Image Cache Service

### In a ViewModel

```csharp
public class PostCardViewModel : ViewModelBase
{
    private readonly IImageCacheService _imageCacheService;
    private BitmapImage _postImage;

    public BitmapImage PostImage
    {
        get => _postImage;
        set => SetProperty(ref _postImage, value);
    }

    public async Task LoadImagesAsync(string imageUrl)
    {
        try
        {
            // Lazy load and cache image (returns byte array)
            var imageData = await _imageCacheService.LoadImageAsync(imageUrl);
            
            if (imageData != null)
            {
                // Convert byte array to BitmapImage
                PostImage = ByteArrayToBitmapImage(imageData);
            }
        }
        catch (Exception ex)
        {
            // Handle error - show placeholder
            Log.Warning(ex, "Failed to load image: {Url}", imageUrl);
        }
    }

    private BitmapImage ByteArrayToBitmapImage(byte[] imageData)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = new MemoryStream(imageData);
        bitmap.EndInit();
        bitmap.Freeze(); // Make it thread-safe
        return bitmap;
    }

    // Preload images for better UX
    public async Task PreloadNextPageImagesAsync(List<string> imageUrls)
    {
        await _imageCacheService.PreloadImagesAsync(imageUrls.ToArray());
    }
}
```

### In XAML

```xml
<Image Source="{Binding PostImage}"
       Stretch="UniformToFill"
       Width="400"
       Height="300"/>
```

## 3. Using Debounced Actions

### For Search Input

```csharp
public class SearchViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private readonly DebouncedAction _searchDebouncer;
    private string _searchQuery;
    private ObservableCollection<SearchResult> _searchResults;

    public SearchViewModel(ISearchService searchService)
    {
        _searchService = searchService;
        _searchDebouncer = new DebouncedAction(300); // 300ms delay
        _searchResults = new ObservableCollection<SearchResult>();
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                // Debounce the search - only executes after user stops typing
                _searchDebouncer.Debounce(async () => await PerformSearchAsync());
            }
        }
    }

    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            return;
        }

        try
        {
            var results = await _searchService.SearchAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Search failed");
        }
    }

    public ObservableCollection<SearchResult> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }
}
```

### For Auto-Save

```csharp
public class JournalEntryViewModel : ViewModelBase
{
    private readonly DebouncedAction _autoSaveDebouncer;
    private string _content;

    public JournalEntryViewModel()
    {
        _autoSaveDebouncer = new DebouncedAction(2000); // 2 second delay
    }

    public string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                // Auto-save after 2 seconds of no typing
                _autoSaveDebouncer.Debounce(async () => await AutoSaveAsync());
            }
        }
    }

    private async Task AutoSaveAsync()
    {
        // Save draft
        await _journalService.SaveDraftAsync(Content);
    }
}
```

## 4. Optimizing Repository Queries

### Read-Only Queries

Always use `AsNoTracking()` for read-only queries:

```csharp
public class PostRepository : IPostRepository
{
    private readonly TogetherDbContext _context;

    // ✅ GOOD - Read-only query
    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _context.Posts
            .AsNoTracking()  // No tracking needed
            .Include(p => p.Author)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // ✅ GOOD - Query for update
    public async Task<Post?> GetForUpdateAsync(Guid id)
    {
        return await _context.Posts
            // No AsNoTracking - we need to track changes
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### Pagination

Always implement pagination for list queries:

```csharp
public async Task<IEnumerable<Post>> GetFeedPostsAsync(Guid userId, int page, int pageSize = 20)
{
    var skip = (page - 1) * pageSize;
    
    return await _context.Posts
        .AsNoTracking()
        .Include(p => p.Author)
        .Include(p => p.Images)
        .Where(p => followingIds.Contains(p.AuthorId))
        .OrderByDescending(p => p.CreatedAt)
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync();
}
```

### Projection to DTOs

Project to DTOs in the database for better performance:

```csharp
// ❌ BAD - Loads full entities then maps
public async Task<IEnumerable<UserDto>> GetUsersAsync()
{
    var users = await _context.Users.ToListAsync();
    return users.Select(u => new UserDto { ... });
}

// ✅ GOOD - Projects in database
public async Task<IEnumerable<UserDto>> GetUsersAsync()
{
    return await _context.Users
        .AsNoTracking()
        .Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email.Value,
            ProfilePictureUrl = u.ProfilePictureUrl
        })
        .ToListAsync();
}
```

## 5. Implementing Virtualization in Views

### Basic Implementation

```xml
<ScrollViewer VerticalScrollBarVisibility="Auto">
    <ItemsControl ItemsSource="{Binding Items}"
                 VirtualizingPanel.IsVirtualizing="True"
                 VirtualizingPanel.VirtualizationMode="Recycling">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <VirtualizingStackPanel/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <!-- Your item template -->
                <Border Padding="8">
                    <TextBlock Text="{Binding Title}"/>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

### With Infinite Scroll

```csharp
public class FeedViewModel : ViewModelBase
{
    private ObservableCollection<PostDto> _posts;
    private bool _isLoadingMore;
    private int _currentPage = 1;

    public ObservableCollection<PostDto> Posts
    {
        get => _posts;
        set => SetProperty(ref _posts, value);
    }

    public ICommand LoadMoreCommand { get; }

    public FeedViewModel()
    {
        _posts = new ObservableCollection<PostDto>();
        LoadMoreCommand = new RelayCommand(async _ => await LoadMoreAsync());
    }

    private async Task LoadMoreAsync()
    {
        if (_isLoadingMore) return;

        _isLoadingMore = true;
        try
        {
            var newPosts = await _feedService.GetFeedAsync(_currentPage, 20);
            foreach (var post in newPosts)
            {
                Posts.Add(post);
            }
            _currentPage++;
        }
        finally
        {
            _isLoadingMore = false;
        }
    }
}
```

## 6. Cache Invalidation Patterns

### Single Item Update

```csharp
public async Task UpdatePostAsync(Guid postId, UpdatePostDto dto)
{
    // Update in database
    await _postRepository.UpdateAsync(post);
    
    // Invalidate specific post cache
    _cacheService.Remove($"post_{postId}");
    
    // Invalidate author's feed cache
    _cacheService.Remove($"feed_{post.AuthorId}_0");
}
```

### Bulk Invalidation

```csharp
public async Task CreatePostAsync(CreatePostDto dto)
{
    var post = await _postRepository.AddAsync(newPost);
    
    // Invalidate all followers' feed caches
    var followerIds = await _followService.GetFollowerIdsAsync(dto.AuthorId);
    foreach (var followerId in followerIds)
    {
        _cacheService.Remove($"feed_{followerId}_0");
    }
}
```

### Time-Based Invalidation

```csharp
// Cache with short expiration for frequently changing data
var recentPosts = await _cacheService.GetOrCreateAsync(
    "recent_posts",
    async () => await _postRepository.GetRecentAsync(10),
    TimeSpan.FromMinutes(2) // Short expiration
);

// Cache with long expiration for static data
var userProfile = await _cacheService.GetOrCreateAsync(
    $"user_profile_{userId}",
    async () => await _userRepository.GetByIdAsync(userId),
    TimeSpan.FromHours(1) // Long expiration
);
```

## 7. Performance Monitoring

### Log Cache Performance

```csharp
public class CachedUserService
{
    private readonly IMemoryCacheService _cacheService;
    private readonly ILogger<CachedUserService> _logger;

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var cacheKey = $"user_{userId}";
        var stopwatch = Stopwatch.StartNew();
        
        var isCached = _cacheService.Exists(cacheKey);
        var user = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await LoadUserFromDatabaseAsync(userId)
        );
        
        stopwatch.Stop();
        _logger.LogInformation(
            "GetUser {UserId} - Cached: {IsCached}, Time: {ElapsedMs}ms",
            userId, isCached, stopwatch.ElapsedMilliseconds
        );
        
        return user;
    }
}
```

### Monitor Image Cache Size

```csharp
public class ImageCacheMonitor
{
    private readonly IImageCacheService _imageCacheService;
    private readonly ILogger<ImageCacheMonitor> _logger;

    public void MonitorCacheSize()
    {
        var cacheSize = _imageCacheService.GetCacheSize();
        var cacheSizeMB = cacheSize / (1024.0 * 1024.0);
        
        _logger.LogInformation("Image cache size: {SizeMB:F2} MB", cacheSizeMB);
        
        // Clear if too large
        if (cacheSizeMB > 100)
        {
            _logger.LogWarning("Image cache exceeded 100 MB, clearing...");
            _imageCacheService.ClearCache();
        }
    }
}
```

## 8. Testing Performance Optimizations

### Unit Test with Cache

```csharp
[Fact]
public async Task GetUser_WithCache_ReturnsCachedValue()
{
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var cacheService = new MemoryCacheService();
    var service = new UserService(mockRepo.Object, cacheService);
    var userId = Guid.NewGuid();
    
    // Act - First call loads from database
    var user1 = await service.GetUserAsync(userId);
    
    // Act - Second call uses cache
    var user2 = await service.GetUserAsync(userId);
    
    // Assert - Repository called only once
    mockRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
}
```

### Integration Test with Database

```csharp
[Fact]
public async Task GetFeedPosts_WithIndexes_PerformsWell()
{
    // Arrange
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var posts = await _postRepository.GetFeedPostsAsync(userId, 0, 20);
    
    stopwatch.Stop();
    
    // Assert - Should complete in under 200ms with indexes
    Assert.True(stopwatch.ElapsedMilliseconds < 200);
}
```

## Common Patterns

### Pattern 1: Cache-Aside

```csharp
public async Task<T> GetWithCacheAsync<T>(string key, Func<Task<T>> loader)
{
    // Try cache first
    var cached = _cacheService.Get<T>(key);
    if (cached != null) return cached;
    
    // Load from source
    var data = await loader();
    
    // Store in cache
    _cacheService.Set(key, data, TimeSpan.FromMinutes(10));
    
    return data;
}
```

### Pattern 2: Write-Through Cache

```csharp
public async Task UpdateWithCacheAsync<T>(string key, T data, Func<Task> updater)
{
    // Update source
    await updater();
    
    // Update cache
    _cacheService.Set(key, data, TimeSpan.FromMinutes(10));
}
```

### Pattern 3: Cache Invalidation on Write

```csharp
public async Task UpdateAndInvalidateAsync(Func<Task> updater, params string[] cacheKeys)
{
    // Update source
    await updater();
    
    // Invalidate related caches
    foreach (var key in cacheKeys)
    {
        _cacheService.Remove(key);
    }
}
```

## Troubleshooting

### Cache Not Working
- Verify service is registered as singleton
- Check cache key consistency
- Verify expiration times are appropriate

### Images Not Loading
- Check image URLs are valid
- Verify network connectivity
- Check cache size limits

### Virtualization Not Working
- Ensure VirtualizingStackPanel is in ItemsPanel
- Verify IsVirtualizing="True"
- Check ScrollViewer is parent

### Slow Queries
- Verify indexes are applied
- Check AsNoTracking is used
- Ensure pagination is implemented

## Summary

✅ Use `IMemoryCacheService` for frequently accessed data
✅ Use `IImageCacheService` for image lazy loading
✅ Use `DebouncedAction` for user input actions
✅ Use `AsNoTracking()` for read-only queries
✅ Use `VirtualizingStackPanel` for long lists
✅ Always implement pagination
✅ Invalidate cache on data changes
✅ Monitor cache sizes and performance
