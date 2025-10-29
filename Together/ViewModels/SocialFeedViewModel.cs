using System.Collections.ObjectModel;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Services;

namespace Together.Presentation.ViewModels;

public class SocialFeedViewModel : ViewModelBase, INavigationAware
{
    private readonly ISocialFeedService _socialFeedService;
    private readonly IPostService _postService;
    private readonly ILikeService _likeService;
    private readonly ICommentService _commentService;
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private readonly IOfflineSyncManager? _offlineSyncManager;
    private Guid _currentUserId;
    private bool _isLoading;
    private bool _isRefreshing;
    private bool _hasMorePosts;
    private int _currentSkip;
    private const int PageSize = 20;
    private string _errorMessage = string.Empty;
    private bool _showSuggestedUsers;

    public SocialFeedViewModel(
        ISocialFeedService socialFeedService, 
        IPostService postService,
        ILikeService likeService,
        ICommentService commentService,
        IRealTimeSyncService? realTimeSyncService = null,
        IOfflineSyncManager? offlineSyncManager = null)
    {
        _socialFeedService = socialFeedService;
        _postService = postService;
        _likeService = likeService;
        _commentService = commentService;
        _realTimeSyncService = realTimeSyncService;
        _offlineSyncManager = offlineSyncManager;
        _currentSkip = 0;
        _hasMorePosts = true;

        Posts = new ObservableCollection<PostCardViewModel>();
        SuggestedUsers = new ObservableCollection<UserDto>();

        LoadFeedCommand = new RelayCommand(async _ => await LoadFeedAsync(), _ => !IsLoading);
        LoadMoreCommand = new RelayCommand(async _ => await LoadMoreAsync(), _ => !IsLoading && HasMorePosts);
        RefreshCommand = new RelayCommand(async _ => await RefreshFeedAsync(), _ => !IsRefreshing);
    }

    public void OnNavigatedTo(object? parameter)
    {
        // Get current user from application properties
        var currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as UserDto;
        if (currentUser != null)
        {
            _currentUserId = currentUser.Id;
            _ = LoadFeedAsync();
        }
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if needed
    }

        // Subscribe to real-time updates
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.PostReceived += OnPostReceived;
        }

        // Load initial feed
        _ = LoadFeedAsync();
    }

    public ObservableCollection<PostCardViewModel> Posts { get; }
    public ObservableCollection<UserDto> SuggestedUsers { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public bool HasMorePosts
    {
        get => _hasMorePosts;
        set => SetProperty(ref _hasMorePosts, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool ShowSuggestedUsers
    {
        get => _showSuggestedUsers;
        set => SetProperty(ref _showSuggestedUsers, value);
    }

    public ICommand LoadFeedCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadFeedAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _currentSkip = 0;

            IEnumerable<PostDto> posts;

            // Try to load from server, fall back to cache if offline
            if (_offlineSyncManager != null && !await _offlineSyncManager.IsOnlineAsync())
            {
                var cachedPosts = await _offlineSyncManager.GetCachedPostsAsync(100);
                posts = cachedPosts.Cast<PostDto>();
                HasMorePosts = false; // No pagination for cached posts
            }
            else
            {
                var result = await _socialFeedService.GetFeedAsync(_currentUserId, _currentSkip, PageSize);
                posts = result.Posts;
                HasMorePosts = result.HasMore;

                // Cache the posts for offline use
                if (_offlineSyncManager != null)
                {
                    await _offlineSyncManager.CachePostsAsync(posts.Cast<object>());
                }
            }

            Posts.Clear();
            foreach (var post in posts)
            {
                var isLiked = await _likeService.IsLikedByUserAsync(post.Id, _currentUserId);
                var postCardViewModel = new PostCardViewModel(_postService, _likeService, _commentService, _currentUserId, post, isLiked);
                postCardViewModel.PostDeleted += OnPostDeleted;
                Posts.Add(postCardViewModel);
            }

            _currentSkip = Posts.Count;

            // If no posts, show suggested users
            if (Posts.Count == 0)
            {
                await LoadSuggestedUsersAsync();
                ShowSuggestedUsers = true;
            }
            else
            {
                ShowSuggestedUsers = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load feed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMoreAsync()
    {
        if (IsLoading || !HasMorePosts) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _socialFeedService.GetFeedAsync(_currentUserId, _currentSkip, PageSize);

            foreach (var post in result.Posts)
            {
                var isLiked = await _likeService.IsLikedByUserAsync(post.Id, _currentUserId);
                var postCardViewModel = new PostCardViewModel(_postService, _likeService, _commentService, _currentUserId, post, isLiked);
                postCardViewModel.PostDeleted += OnPostDeleted;
                Posts.Add(postCardViewModel);
            }

            HasMorePosts = result.HasMore;
            _currentSkip = Posts.Count;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load more posts: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshFeedAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            // Invalidate cache
            await _socialFeedService.RefreshFeedCacheAsync(_currentUserId);

            // Reload feed
            _currentSkip = 0;
            var result = await _socialFeedService.GetFeedAsync(_currentUserId, _currentSkip, PageSize);

            Posts.Clear();
            foreach (var post in result.Posts)
            {
                var isLiked = await _likeService.IsLikedByUserAsync(post.Id, _currentUserId);
                var postCardViewModel = new PostCardViewModel(_postService, _likeService, _commentService, _currentUserId, post, isLiked);
                postCardViewModel.PostDeleted += OnPostDeleted;
                Posts.Add(postCardViewModel);
            }

            HasMorePosts = result.HasMore;
            _currentSkip = Posts.Count;

            // Update suggested users visibility
            if (Posts.Count == 0)
            {
                await LoadSuggestedUsersAsync();
                ShowSuggestedUsers = true;
            }
            else
            {
                ShowSuggestedUsers = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to refresh feed: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task LoadSuggestedUsersAsync()
    {
        try
        {
            var suggestedUsers = await _socialFeedService.GetSuggestedUsersAsync(_currentUserId, 5);

            SuggestedUsers.Clear();
            foreach (var user in suggestedUsers)
            {
                SuggestedUsers.Add(user);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load suggested users: {ex.Message}";
        }
    }

    private void OnPostDeleted(object? sender, Guid postId)
    {
        var postToRemove = Posts.FirstOrDefault(p => p.Post.Id == postId);
        if (postToRemove != null)
        {
            postToRemove.PostDeleted -= OnPostDeleted;
            Posts.Remove(postToRemove);
            _currentSkip = Posts.Count;
        }
    }

    private void OnPostReceived(object? sender, PostDto post)
    {
        // Check if post already exists
        if (Posts.Any(p => p.Post.Id == post.Id))
            return;

        // Add the new post to the feed on UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        {
            try
            {
                var isLiked = await _likeService.IsLikedByUserAsync(post.Id, _currentUserId);
                var postCardViewModel = new PostCardViewModel(_postService, _likeService, _commentService, _currentUserId, post, isLiked);
                postCardViewModel.PostDeleted += OnPostDeleted;
                Posts.Insert(0, postCardViewModel);
                _currentSkip = Posts.Count;

                // Hide suggested users if we now have posts
                if (ShowSuggestedUsers && Posts.Count > 0)
                {
                    ShowSuggestedUsers = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add new post: {ex.Message}";
            }
        });
    }

    public void Dispose()
    {
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.PostReceived -= OnPostReceived;
        }

        foreach (var post in Posts)
        {
            post.PostDeleted -= OnPostDeleted;
        }
    }
}
