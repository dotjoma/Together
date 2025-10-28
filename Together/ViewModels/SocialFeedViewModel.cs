using System.Collections.ObjectModel;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class SocialFeedViewModel : ViewModelBase
{
    private readonly ISocialFeedService _socialFeedService;
    private readonly IPostService _postService;
    private readonly ILikeService _likeService;
    private readonly ICommentService _commentService;
    private readonly Guid _currentUserId;
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
        Guid currentUserId)
    {
        _socialFeedService = socialFeedService;
        _postService = postService;
        _likeService = likeService;
        _commentService = commentService;
        _currentUserId = currentUserId;
        _currentSkip = 0;
        _hasMorePosts = true;

        Posts = new ObservableCollection<PostCardViewModel>();
        SuggestedUsers = new ObservableCollection<UserDto>();

        LoadFeedCommand = new RelayCommand(async _ => await LoadFeedAsync(), _ => !IsLoading);
        LoadMoreCommand = new RelayCommand(async _ => await LoadMoreAsync(), _ => !IsLoading && HasMorePosts);
        RefreshCommand = new RelayCommand(async _ => await RefreshFeedAsync(), _ => !IsRefreshing);

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
}
