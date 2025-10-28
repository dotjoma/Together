using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class FollowerListViewModel : ViewModelBase
{
    private readonly IFollowService _followService;
    private ObservableCollection<FollowRelationshipDto> _followers;
    private int _followerCount;
    private bool _isLoading;

    public ObservableCollection<FollowRelationshipDto> Followers
    {
        get => _followers;
        set => SetProperty(ref _followers, value);
    }

    public int FollowerCount
    {
        get => _followerCount;
        set => SetProperty(ref _followerCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasNoFollowers => Followers?.Count == 0;

    public ICommand ViewProfileCommand { get; }
    public ICommand RefreshCommand { get; }

    public FollowerListViewModel(IFollowService followService, Guid userId)
    {
        _followService = followService;
        _followers = new ObservableCollection<FollowRelationshipDto>();

        ViewProfileCommand = new RelayCommand(param => ViewProfile((Guid)param!));
        RefreshCommand = new RelayCommand(async _ => await LoadFollowersAsync(userId));

        _ = LoadFollowersAsync(userId);
    }

    private async Task LoadFollowersAsync(Guid userId)
    {
        try
        {
            IsLoading = true;
            var followers = await _followService.GetFollowersAsync(userId);
            Followers = new ObservableCollection<FollowRelationshipDto>(followers);
            FollowerCount = await _followService.GetFollowerCountAsync(userId);
            OnPropertyChanged(nameof(HasNoFollowers));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load followers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ViewProfile(Guid userId)
    {
        // TODO: Navigate to user profile
        MessageBox.Show($"Navigate to profile: {userId}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
