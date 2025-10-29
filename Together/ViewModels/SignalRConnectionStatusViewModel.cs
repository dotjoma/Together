using Together.Application.Interfaces;

namespace Together.Presentation.ViewModels;

/// <summary>
/// ViewModel for displaying SignalR real-time connection status
/// </summary>
public class SignalRConnectionStatusViewModel : ViewModelBase
{
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private bool _isConnected;
    private string _statusText = "Disconnected";
    private string _statusColor = "#F44336";

    public SignalRConnectionStatusViewModel(IRealTimeSyncService? realTimeSyncService = null)
    {
        _realTimeSyncService = realTimeSyncService;

        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.ConnectionStatusChanged += OnConnectionStatusChanged;
            UpdateStatus(_realTimeSyncService.IsConnected);
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    private void OnConnectionStatusChanged(object? sender, bool isConnected)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            UpdateStatus(isConnected);
        });
    }

    private void UpdateStatus(bool isConnected)
    {
        IsConnected = isConnected;
        StatusText = isConnected ? "Connected" : "Disconnected";
        StatusColor = isConnected ? "#4CAF50" : "#F44336";
    }

    public void Dispose()
    {
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.ConnectionStatusChanged -= OnConnectionStatusChanged;
        }
    }
}
