using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class LikeButtonViewModel : ViewModelBase
{
    private readonly ILikeService _likeService;
    private readonly Guid _postId;
    private readonly Guid _currentUserId;
    private bool _isLiked;
    private int _likeCount;
    private bool _isProcessing;

    public LikeButtonViewModel(ILikeService likeService, Guid postId, Guid currentUserId, int initialLikeCount, bool isLiked)
    {
        _likeService = likeService;
        _postId = postId;
        _currentUserId = currentUserId;
        _likeCount = initialLikeCount;
        _isLiked = isLiked;

        ToggleLikeCommand = new RelayCommand(async _ => await ToggleLikeAsync(), _ => !_isProcessing);
    }

    public ICommand ToggleLikeCommand { get; }

    public bool IsLiked
    {
        get => _isLiked;
        set => SetProperty(ref _isLiked, value);
    }

    public int LikeCount
    {
        get => _likeCount;
        set => SetProperty(ref _likeCount, value);
    }

    public PackIconKind IconKind => IsLiked ? PackIconKind.Heart : PackIconKind.HeartOutline;

    public Brush IconColor => IsLiked 
        ? new SolidColorBrush(Color.FromRgb(244, 67, 54)) // Red color for liked
        : new SolidColorBrush(Colors.Gray);

    private async Task ToggleLikeAsync()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        try
        {
            var liked = await _likeService.ToggleLikeAsync(_postId, _currentUserId);
            IsLiked = liked;
            LikeCount = liked ? LikeCount + 1 : LikeCount - 1;
            
            OnPropertyChanged(nameof(IconKind));
            OnPropertyChanged(nameof(IconColor));
        }
        catch (Exception ex)
        {
            // Log error or show notification
            System.Diagnostics.Debug.WriteLine($"Error toggling like: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
