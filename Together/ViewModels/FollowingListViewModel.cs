using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class FollowingListViewModel : ViewModelBase
{
    private readonly IFollowService _followService;
    private readonly Guid _currentUserId;
    private ObservableCollection<FollowRelationshipDto> _following;
    private int _followingCount;
    private bool _isLoading;

    public ObservableCollection<FollowRelationshipDto> Following
    {
        get => _following;
        set => SetProperty(ref _following, value);
    }

    public int FollowingCount
    {
        get => _followingCount;
        set => SetProperty(ref _followingCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasNoFollowing => Following?.Count == 0;

    public ICommand ViewProfileCommand { get; }
    public ICommand UnfollowCommand { get; }
    public ICommand RefreshCommand { get; }

    public FollowingListViewModel(IFollowService followService, Guid currentUserId)
    {
        _followService = followService;
        _currentUserId = currentUserId;
        _following = new ObservableCollection<FollowRelationshipDto>();

        ViewProfileCommand = new RelayCommand(param => ViewProfile((Guid)param!));
        UnfollowCommand = new RelayCommand(async param => await UnfollowAsync((Guid)param!));
        RefreshCommand = new RelayCommand(async _ => await LoadFollowingAsync());

        _ = LoadFollowingAsync();
    }

    private async Task LoadFollowingAsync()
    {
        try
        {
            IsLoading = true;
            var following = await _followService.GetFollowingAsync(_currentUserId);
            Following = new ObservableCollection<FollowRelationshipDto>(following);
            FollowingCount = await _followService.GetFollowingCountAsync(_currentUserId);
            OnPropertyChanged(nameof(HasNoFollowing));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load following: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UnfollowAsync(Guid followingUserId)
    {
        var result = MessageBox.Show(
            "Are you sure you want to unfollow this user?",
            "Confirm Unfollow",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _followService.UnfollowAsync(_currentUserId, followingUserId);
            var relationship = Following.FirstOrDefault(f => f.Following.Id == followingUserId);
            if (relationship != null)
            {
                Following.Remove(relationship);
                FollowingCount--;
                OnPropertyChanged(nameof(HasNoFollowing));
            }
            MessageBox.Show("Successfully unfollowed user.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to unfollow: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ViewProfile(Guid userId)
    {
        // TODO: Navigate to user profile
        MessageBox.Show($"Navigate to profile: {userId}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
