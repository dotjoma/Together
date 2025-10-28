using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class ConnectionRequestNotificationViewModel : ViewModelBase
{
    private readonly ICoupleConnectionService _coupleConnectionService;
    private ObservableCollection<ConnectionRequestDto> _pendingRequests = new();
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public ConnectionRequestNotificationViewModel(ICoupleConnectionService coupleConnectionService)
    {
        _coupleConnectionService = coupleConnectionService;

        AcceptRequestCommand = new RelayCommand(async param => await AcceptRequestAsync(param), _ => !IsLoading);
        RejectRequestCommand = new RelayCommand(async param => await RejectRequestAsync(param), _ => !IsLoading);
        RefreshCommand = new RelayCommand(async _ => await LoadPendingRequestsAsync(), _ => !IsLoading);
    }

    public ObservableCollection<ConnectionRequestDto> PendingRequests
    {
        get => _pendingRequests;
        set => SetProperty(ref _pendingRequests, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RelayCommand AcceptRequestCommand { get; }
    public RelayCommand RejectRequestCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public async Task LoadPendingRequestsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var currentUserId = GetCurrentUserId();
            var requests = await _coupleConnectionService.GetPendingRequestsAsync(currentUserId);

            PendingRequests = new ObservableCollection<ConnectionRequestDto>(requests);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load requests: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AcceptRequestAsync(object? parameter)
    {
        if (parameter is not ConnectionRequestDto request)
            return;

        var result = MessageBox.Show(
            $"Accept connection request from {request.FromUser.Username}?",
            "Accept Connection",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var currentUserId = GetCurrentUserId();
            await _coupleConnectionService.AcceptConnectionRequestAsync(request.Id, currentUserId);

            MessageBox.Show(
                $"You are now connected with {request.FromUser.Username}!",
                "Connection Established",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadPendingRequestsAsync();
        }
        catch (BusinessRuleViolationException ex)
        {
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to accept request: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RejectRequestAsync(object? parameter)
    {
        if (parameter is not ConnectionRequestDto request)
            return;

        var result = MessageBox.Show(
            $"Reject connection request from {request.FromUser.Username}?",
            "Reject Connection",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var currentUserId = GetCurrentUserId();
            await _coupleConnectionService.RejectConnectionRequestAsync(request.Id, currentUserId);

            await LoadPendingRequestsAsync();
        }
        catch (BusinessRuleViolationException ex)
        {
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to reject request: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Implement proper session management
        return System.Windows.Application.Current.Properties.Contains("CurrentUserId") 
            ? (Guid)System.Windows.Application.Current.Properties["CurrentUserId"]! 
            : Guid.Empty;
    }
}
