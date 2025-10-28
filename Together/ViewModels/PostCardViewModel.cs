using System;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class PostCardViewModel : ViewModelBase
{
    private readonly IPostService _postService;
    private readonly ILikeService _likeService;
    private readonly ICommentService _commentService;
    private readonly Guid _currentUserId;
    private PostDto _post;
    private bool _isDeleting;
    private bool _showComments;

    public PostCardViewModel(
        IPostService postService, 
        ILikeService likeService,
        ICommentService commentService,
        Guid currentUserId, 
        PostDto post,
        bool isLiked = false)
    {
        _postService = postService;
        _likeService = likeService;
        _commentService = commentService;
        _currentUserId = currentUserId;
        _post = post;

        EditCommand = new RelayCommand(_ => Edit(), _ => CanEdit());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsDeleting);
        ToggleCommentsCommand = new RelayCommand(_ => ToggleComments());

        LikeButtonViewModel = new LikeButtonViewModel(_likeService, post.Id, currentUserId, post.LikeCount, isLiked);
        CommentSectionViewModel = new CommentSectionViewModel(_commentService, post.Id, currentUserId);
    }

    public PostDto Post
    {
        get => _post;
        set => SetProperty(ref _post, value);
    }

    public bool IsOwnPost => _post.Author.Id == _currentUserId;

    public bool CanEditPost
    {
        get
        {
            if (!IsOwnPost) return false;
            var timeSinceCreation = DateTime.UtcNow - _post.CreatedAt;
            return timeSinceCreation.TotalMinutes <= 15;
        }
    }

    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.UtcNow - _post.CreatedAt;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            return _post.CreatedAt.ToString("MMM dd, yyyy");
        }
    }

    public string EditedText => _post.EditedAt.HasValue ? " (edited)" : string.Empty;

    public bool IsDeleting
    {
        get => _isDeleting;
        private set => SetProperty(ref _isDeleting, value);
    }

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleCommentsCommand { get; }

    public LikeButtonViewModel LikeButtonViewModel { get; }
    public CommentSectionViewModel CommentSectionViewModel { get; }

    public bool ShowComments
    {
        get => _showComments;
        set => SetProperty(ref _showComments, value);
    }

    public event EventHandler<PostDto>? EditRequested;
    public event EventHandler<Guid>? PostDeleted;

    private bool CanEdit()
    {
        return CanEditPost && !IsDeleting;
    }

    private void Edit()
    {
        EditRequested?.Invoke(this, _post);
    }

    private async Task DeleteAsync()
    {
        try
        {
            IsDeleting = true;
            await _postService.DeletePostAsync(_currentUserId, _post.Id);
            PostDeleted?.Invoke(this, _post.Id);
        }
        catch (Exception ex)
        {
            // Handle error - could show a message box or notification
            System.Diagnostics.Debug.WriteLine($"Error deleting post: {ex.Message}");
        }
        finally
        {
            IsDeleting = false;
        }
    }

    public void UpdatePost(PostDto updatedPost)
    {
        Post = updatedPost;
        OnPropertyChanged(nameof(CanEditPost));
        OnPropertyChanged(nameof(TimeAgo));
        OnPropertyChanged(nameof(EditedText));
    }

    private void ToggleComments()
    {
        ShowComments = !ShowComments;
    }
}
