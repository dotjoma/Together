using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Together.Application.DTOs;
using Together.Application.Interfaces;

namespace Together.Infrastructure.SignalR;

/// <summary>
/// SignalR hub client for real-time communication
/// </summary>
public class TogetherHub : IRealTimeSyncService
{
    private HubConnection? _connection;
    private readonly ILogger<TogetherHub>? _logger;
    private Guid _currentUserId;
    private readonly TimeSpan[] _reconnectDelays = new[]
    {
        TimeSpan.FromSeconds(0),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    };

    public event EventHandler<JournalEntryDto>? JournalEntryReceived;
    public event EventHandler<PostDto>? PostReceived;
    public event EventHandler<MoodEntryDto>? MoodEntryReceived;
    public event EventHandler<NotificationDto>? NotificationReceived;
    public event EventHandler<bool>? ConnectionStatusChanged;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public TogetherHub(ILogger<TogetherHub>? logger = null)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(Guid userId, string token)
    {
        if (_connection != null)
        {
            await DisconnectAsync();
        }

        _currentUserId = userId;

        // Build the connection
        _connection = new HubConnectionBuilder()
            .WithUrl("https://your-signalr-hub-url/togetherhub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect(new RetryPolicy(_reconnectDelays))
            .Build();

        // Register event handlers
        RegisterHandlers();

        // Handle connection events
        _connection.Closed += OnConnectionClosed;
        _connection.Reconnecting += OnReconnecting;
        _connection.Reconnected += OnReconnected;

        try
        {
            await _connection.StartAsync();
            _logger?.LogInformation("Connected to SignalR hub for user {UserId}", userId);
            ConnectionStatusChanged?.Invoke(this, true);

            // Join user-specific group
            await _connection.InvokeAsync("JoinUserGroup", userId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to SignalR hub");
            ConnectionStatusChanged?.Invoke(this, false);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            try
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
                _logger?.LogInformation("Disconnected from SignalR hub");
                ConnectionStatusChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disconnecting from SignalR hub");
            }
        }
    }

    public async Task BroadcastToPartnerAsync(string eventType, object data)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot broadcast to partner: not connected");
            return;
        }

        try
        {
            await _connection.InvokeAsync("BroadcastToPartner", _currentUserId, eventType, data);
            _logger?.LogDebug("Broadcasted {EventType} to partner", eventType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to broadcast to partner");
            throw;
        }
    }

    public async Task BroadcastToFollowersAsync(string eventType, object data)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot broadcast to followers: not connected");
            return;
        }

        try
        {
            await _connection.InvokeAsync("BroadcastToFollowers", _currentUserId, eventType, data);
            _logger?.LogDebug("Broadcasted {EventType} to followers", eventType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to broadcast to followers");
            throw;
        }
    }

    public async Task NotifyUserAsync(Guid userId, NotificationDto notification)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot send notification: not connected");
            return;
        }

        try
        {
            await _connection.InvokeAsync("NotifyUser", userId, notification);
            _logger?.LogDebug("Sent notification to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send notification");
            throw;
        }
    }

    private void RegisterHandlers()
    {
        if (_connection == null) return;

        // Journal entry updates
        _connection.On<JournalEntryDto>("ReceiveJournalEntry", entry =>
        {
            _logger?.LogDebug("Received journal entry: {EntryId}", entry.Id);
            JournalEntryReceived?.Invoke(this, entry);
        });

        // Post updates
        _connection.On<PostDto>("ReceivePost", post =>
        {
            _logger?.LogDebug("Received post: {PostId}", post.Id);
            PostReceived?.Invoke(this, post);
        });

        // Mood updates
        _connection.On<MoodEntryDto>("ReceiveMoodEntry", mood =>
        {
            _logger?.LogDebug("Received mood entry: {MoodId}", mood.Id);
            MoodEntryReceived?.Invoke(this, mood);
        });

        // Notifications
        _connection.On<NotificationDto>("ReceiveNotification", notification =>
        {
            _logger?.LogDebug("Received notification: {NotificationId}", notification.Id);
            NotificationReceived?.Invoke(this, notification);
        });
    }

    private Task OnConnectionClosed(Exception? exception)
    {
        _logger?.LogWarning(exception, "SignalR connection closed");
        ConnectionStatusChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    private Task OnReconnecting(Exception? exception)
    {
        _logger?.LogInformation("SignalR reconnecting...");
        ConnectionStatusChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    private Task OnReconnected(string? connectionId)
    {
        _logger?.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
        ConnectionStatusChanged?.Invoke(this, true);

        // Rejoin user group after reconnection
        if (_connection != null)
        {
            _ = _connection.InvokeAsync("JoinUserGroup", _currentUserId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Custom retry policy for exponential backoff
    /// </summary>
    private class RetryPolicy : IRetryPolicy
    {
        private readonly TimeSpan[] _delays;

        public RetryPolicy(TimeSpan[] delays)
        {
            _delays = delays;
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            // If we've exceeded max attempts, stop retrying
            if (retryContext.PreviousRetryCount >= _delays.Length)
            {
                return null;
            }

            return _delays[retryContext.PreviousRetryCount];
        }
    }
}
