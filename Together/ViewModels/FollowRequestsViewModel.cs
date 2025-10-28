using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class FollowRequestsViewModel : ViewModelBase
{
    private readonly IFollowService _followService;
    private ObservableCollection<FollowRelationshipDto> _pendingRequests;
    private bool _isLoading;

    public ObservableCollection<FollowRelationshipDto> PendingRequests
    {
        get => _pendingRequests;
        set => SetProperty(ref _pendingRequests, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasNoPendingRequests => PendingRequests?.Count == 0;

    public ICommand AcceptRequestCommand { get; }
    public ICommand RejectRequestCommand { get; }
    public ICommand RefreshCommand { get; }

    public FollowRequestsViewModel(IFollowService followService, Guid currentUserId)
    {
        _followService = followService;
        _pendingRequests = new ObservableCollection<FollowRelationshipDto>();

        AcceptRequestCommand = new RelayCommand(async param => await AcceptRequestAsync((Guid)param!));
        RejectRequestCommand = new RelayCommand(async param => await RejectRequestAsync((Guid)param!));
        RefreshCommand = new RelayCommand(async _ => await LoadPendingRequestsAsync(currentUserId));

        _ = LoadPendingRequestsAsync(currentUserId);
    }

    private async Task LoadPendingRequestsAsync(Guid userId)
    {
        try
        {
            IsLoading = true;
            var requests = await _followService.GetPendingRequestsAsync(userId);
            PendingRequests = new ObservableCollection<FollowRelationshipDto>(requests);
            OnPropertyChanged(nameof(HasNoPendingRequests));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load pending requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AcceptRequestAsync(Guid requestId)
    {
        try
        {
            await _followService.AcceptFollowRequestAsync(requestId);
            var request = PendingRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null)
            {
                PendingRequests.Remove(request);
                OnPropertyChanged(nameof(HasNoPendingRequests));
            }
            MessageBox.Show("Follow request accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to accept request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RejectRequestAsync(Guid requestId)
    {
        try
        {
            await _followService.RejectFollowRequestAsync(requestId);
            var request = PendingRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null)
            {
                PendingRequests.Remove(request);
                OnPropertyChanged(nameof(HasNoPendingRequests));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to reject request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
