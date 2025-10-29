using Together.Application.DTOs;

namespace Together.Application.Interfaces;

/// <summary>
/// Service for managing real-time synchronization via SignalR
/// </summary>
public interface IRealTimeSyncService
{
    /// <summary>
    /// Connects to the SignalR hub
    /// </summary>
    Task ConnectAsync(Guid userId, string token);

    /// <summary>
    /// Disconnects from the SignalR hub
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Gets the current connection status
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Broadcasts an update to the user's partner
    /// </summary>
    Task BroadcastToPartnerAsync(string eventType, object data);

    /// <summary>
    /// Broadcasts an update to all followers
    /// </summary>
    Task BroadcastToFollowersAsync(string eventType, object data);

    /// <summary>
    /// Sends a notification to a specific user
    /// </summary>
    Task NotifyUserAsync(Guid userId, NotificationDto notification);

    /// <summary>
    /// Event raised when a journal entry is received
    /// </summary>
    event EventHandler<JournalEntryDto>? JournalEntryReceived;

    /// <summary>
    /// Event raised when a post is received
    /// </summary>
    event EventHandler<PostDto>? PostReceived;

    /// <summary>
    /// Event raised when a mood entry is received
    /// </summary>
    event EventHandler<MoodEntryDto>? MoodEntryReceived;

    /// <summary>
    /// Event raised when a notification is received
    /// </summary>
    event EventHandler<NotificationDto>? NotificationReceived;

    /// <summary>
    /// Event raised when connection status changes
    /// </summary>
    event EventHandler<bool>? ConnectionStatusChanged;
}
