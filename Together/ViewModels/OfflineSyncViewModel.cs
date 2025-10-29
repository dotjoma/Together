using System.Windows.Threading;
using Together.Application.Interfaces;

namespace Together.Presentation.ViewModels;

/// <summary>
/// ViewModel for managing offline and sync status display
/// </summary>
public class OfflineSyncViewModel : ViewModelBase
{
    private readonly IOfflineSyncManager _offlineSyncManager;
    private readonly IRealTimeSyncService _realTimeSyncService;
    private readonly DispatcherTimer _statusCheckTimer;
    private readonly DispatcherTimer _syncTimer;

    private bool _isOffline;
    private bool _isSyncing;
    private int _pendingOperationCount;
    private int _totalOperations;
    private int _completedOperations;
    private string? _currentOperation;

    public bool IsOffline
    {
        get => _isOffline;
        set => SetProperty(ref _isOffline, value);
    }

    public bool IsSyncing
    {
        get => _isSyncing;
        set => SetProperty(ref _isSyncing, value);
    }

    public int PendingOperationCount
    {
        get => _pendingOperationCount;
        set => SetProperty(ref _pendingOperationCount, value);
    }

    public int TotalOperations
    {
        get => _totalOperations;
        set => SetProperty(ref _totalOperations, value);
    }

    public int CompletedOperations
    {
        get => _completedOperations;
        set
        {
            SetProperty(ref _completedOperations, value);
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }

    public string? CurrentOperation
    {
        get => _currentOperation;
        set => SetProperty(ref _currentOperation, value);
    }

    public int ProgressPercentage
    {
        get
        {
            if (TotalOperations == 0) return 0;
            return (int)((double)CompletedOperations / TotalOperations * 100);
        }
    }

    public OfflineSyncViewModel(
        IOfflineSyncManager offlineSyncManager,
        IRealTimeSyncService realTimeSyncService)
    {
        _offlineSyncManager = offlineSyncManager;
        _realTimeSyncService = realTimeSyncService;

        // Subscribe to sync status changes
        _offlineSyncManager.SyncStatusChanged += OnSyncStatusChanged;
        _realTimeSyncService.ConnectionStatusChanged += OnConnectionStatusChanged;

        // Set up timer to check online status every 10 seconds
        _statusCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _statusCheckTimer.Tick += async (s, e) => await CheckOnlineStatusAsync();
        _statusCheckTimer.Start();

        // Set up timer to sync pending operations every 30 seconds when online
        _syncTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _syncTimer.Tick += async (s, e) => await TrySyncAsync();
        _syncTimer.Start();

        // Initial status check
        _ = CheckOnlineStatusAsync();
    }

    private async Task CheckOnlineStatusAsync()
    {
        try
        {
            var isOnline = await _offlineSyncManager.IsOnlineAsync();
            IsOffline = !isOnline;

            if (!IsOffline)
            {
                // Update pending operation count
                PendingOperationCount = await _offlineSyncManager.GetPendingOperationCountAsync();
            }
        }
        catch (Exception)
        {
            // Assume offline if check fails
            IsOffline = true;
        }
    }

    private async Task TrySyncAsync()
    {
        if (IsOffline || IsSyncing || PendingOperationCount == 0)
        {
            return;
        }

        try
        {
            await _offlineSyncManager.SyncPendingOperationsAsync();
            
            // Update pending count after sync
            PendingOperationCount = await _offlineSyncManager.GetPendingOperationCountAsync();
        }
        catch (Exception)
        {
            // Sync failed, will retry on next timer tick
        }
    }

    private void OnSyncStatusChanged(object? sender, SyncStatusChangedEventArgs e)
    {
        IsSyncing = e.IsSyncing;
        TotalOperations = e.TotalOperations;
        CompletedOperations = e.CompletedOperations;
        CurrentOperation = e.CurrentOperation;
    }

    private void OnConnectionStatusChanged(object? sender, bool isConnected)
    {
        IsOffline = !isConnected;

        // If we just came online, trigger a sync
        if (isConnected && PendingOperationCount > 0)
        {
            _ = TrySyncAsync();
        }
    }

    public async Task ManualSyncAsync()
    {
        if (!IsOffline && !IsSyncing)
        {
            await TrySyncAsync();
        }
    }

    public void Dispose()
    {
        _statusCheckTimer?.Stop();
        _syncTimer?.Stop();
        _offlineSyncManager.SyncStatusChanged -= OnSyncStatusChanged;
        _realTimeSyncService.ConnectionStatusChanged -= OnConnectionStatusChanged;
    }
}
