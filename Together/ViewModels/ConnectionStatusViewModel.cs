using System;
using System.Threading.Tasks;
using System.Windows;
using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class ConnectionStatusViewModel : ViewModelBase
{
    private readonly ICoupleConnectionService _coupleConnectionService;
    private CoupleConnectionDto? _currentConnection;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public ConnectionStatusViewModel(ICoupleConnectionService coupleConnectionService)
    {
        _coupleConnectionService = coupleConnectionService;

        TerminateConnectionCommand = new RelayCommand(async _ => await TerminateConnectionAsync(), _ => !IsLoading && HasConnection);
        RefreshCommand = new RelayCommand(async _ => await LoadConnectionStatusAsync(), _ => !IsLoading);
    }

    public CoupleConnectionDto? CurrentConnection
    {
        get => _currentConnection;
        set
        {
            SetProperty(ref _currentConnection, value);
            OnPropertyChanged(nameof(HasConnection));
            OnPropertyChanged(nameof(PartnerName));
            OnPropertyChanged(nameof(ConnectionInfo));
        }
    }

    public bool HasConnection => CurrentConnection != null;

    public string PartnerName
    {
        get
        {
            if (CurrentConnection == null)
                return string.Empty;

            var currentUserId = GetCurrentUserId();
            return CurrentConnection.User1.Id == currentUserId 
                ? CurrentConnection.User2.Username 
                : CurrentConnection.User1.Username;
        }
    }

    public string ConnectionInfo
    {
        get
        {
            if (CurrentConnection == null)
                return "No active connection";

            var daysTogether = (DateTime.UtcNow - CurrentConnection.RelationshipStartDate).Days;
            return $"Connected with {PartnerName} • {daysTogether} days together • {CurrentConnection.LoveStreak} day streak";
        }
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

    public RelayCommand TerminateConnectionCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public async Task LoadConnectionStatusAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var currentUserId = GetCurrentUserId();
            CurrentConnection = await _coupleConnectionService.GetUserConnectionAsync(currentUserId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load connection status: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task TerminateConnectionAsync()
    {
        if (CurrentConnection == null)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to end your connection with {PartnerName}? This will archive all shared data.",
            "Terminate Connection",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var currentUserId = GetCurrentUserId();
            await _coupleConnectionService.TerminateConnectionAsync(CurrentConnection.Id, currentUserId);

            MessageBox.Show(
                "Connection terminated successfully.",
                "Connection Terminated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            CurrentConnection = null;
        }
        catch (BusinessRuleViolationException ex)
        {
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to terminate connection: {ex.Message}";
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
