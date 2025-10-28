using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class CommentSectionViewModel : ViewModelBase
{
    private readonly ICommentService _commentService;
    private readonly Guid _postId;
    private readonly Guid _currentUserId;
    private string _commentText = string.Empty;
    private bool _isLoading;

    public CommentSectionViewModel(ICommentService commentService, Guid postId, Guid currentUserId)
    {
        _commentService = commentService;
        _postId = postId;
        _currentUserId = currentUserId;

        Comments = new ObservableCollection<CommentViewModel>();
        AddCommentCommand = new RelayCommand(async _ => await AddCommentAsync(), _ => CanAddComment);

        _ = LoadCommentsAsync();
    }

    public ObservableCollection<CommentViewModel> Comments { get; }

    public ICommand AddCommentCommand { get; }

    public string CommentText
    {
        get => _commentText;
        set
        {
            SetProperty(ref _commentText, value);
            OnPropertyChanged(nameof(CharacterCount));
            OnPropertyChanged(nameof(CanAddComment));
        }
    }

    public string CharacterCount => $"{CommentText.Length}/300";

    public bool CanAddComment => !string.IsNullOrWhiteSpace(CommentText) && CommentText.Length <= 300 && !IsLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private async Task LoadCommentsAsync()
    {
        IsLoading = true;
        try
        {
            var comments = await _commentService.GetCommentsAsync(_postId, 0, 50);
            Comments.Clear();
            foreach (var comment in comments)
            {
                Comments.Add(new CommentViewModel(comment));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading comments: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddCommentAsync()
    {
        if (!CanAddComment) return;

        IsLoading = true;
        try
        {
            var dto = new CreateCommentDto(_postId, CommentText);
            var comment = await _commentService.AddCommentAsync(_currentUserId, dto);
            
            Comments.Add(new CommentViewModel(comment));
            CommentText = string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding comment: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class CommentViewModel : ViewModelBase
{
    private readonly CommentDto _comment;

    public CommentViewModel(CommentDto comment)
    {
        _comment = comment;
    }

    public UserDto Author => _comment.Author;
    public string Content => _comment.Content;
    public DateTime CreatedAt => _comment.CreatedAt;

    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.UtcNow - CreatedAt;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            return CreatedAt.ToString("MMM dd, yyyy");
        }
    }
}
