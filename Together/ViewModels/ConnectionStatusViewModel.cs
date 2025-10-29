using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

/// <summary>
/// ViewModel for displaying couple connection status
/// </summary>
public class ConnectionStatusViewModel : ViewModelBase
{
    private readonly ICoupleConnectionService _coupleConnectionService;
    private readonly Guid _userId;
    private bool _isLoading;
    private bool _hasConnection;
    private string? _connectionInfo;
    private string? _errorMessage;

    public ConnectionStatusViewModel(
        ICoupleConnectionService coupleConnectionService,
        Guid userId)
    {
        _coupleConnectionService = coupleConnectionService;
        _userId = userId;

        RefreshCommand = new RelayCommand(async _ => await LoadConnectionStatusAsync());
        TerminateConnectionCommand = new RelayCommand(
            async _ => await TerminateConnectionAsync(),
            _ => HasConnection);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasConnection
    {
        get => _hasConnection;
        set => SetProperty(ref _hasConnection, value);
    }

    public string? ConnectionInfo
    {
        get => _connectionInfo;
        set => SetProperty(ref _connectionInfo, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand TerminateConnectionCommand { get; }

    public async Task LoadConnectionStatusAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var connection = await _coupleConnectionService.GetUserConnectionAsync(_userId);

            if (connection != null)
            {
                HasConnection = true;
                var partner = connection.User1.Id == _userId ? connection.User2 : connection.User1;
                ConnectionInfo = $"Connected with {partner.Username} since {connection.EstablishedAt:d}";
            }
            else
            {
                HasConnection = false;
                ConnectionInfo = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load connection status: {ex.Message}";
            HasConnection = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task TerminateConnectionAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var connection = await _coupleConnectionService.GetUserConnectionAsync(_userId);
            if (connection != null)
            {
                await _coupleConnectionService.TerminateConnectionAsync(connection.Id, _userId);
                HasConnection = false;
                ConnectionInfo = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to terminate connection: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
