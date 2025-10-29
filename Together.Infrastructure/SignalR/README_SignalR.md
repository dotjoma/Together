# SignalR Real-Time Synchronization Implementation

## Overview

This implementation provides real-time synchronization capabilities for the Together application using SignalR. It enables instant updates across clients for journal entries, posts, mood updates, and notifications.

## Architecture

### Components

1. **TogetherHub** (`Together.Infrastructure/SignalR/TogetherHub.cs`)
   - SignalR client implementation
   - Manages connection lifecycle
   - Implements exponential backoff reconnection strategy
   - Handles event subscriptions and broadcasting

2. **IRealTimeSyncService** (`Together.Application/Interfaces/IRealTimeSyncService.cs`)
   - Service interface for real-time operations
   - Defines events for receiving updates
   - Provides methods for broadcasting to partners and followers

3. **ConnectionStatusViewModel** (`Together/ViewModels/ConnectionStatusViewModel.cs`)
   - Displays connection status to users
   - Updates in real-time when connection state changes

4. **ConnectionStatusIndicator** (`Together/Controls/ConnectionStatusIndicator.xaml`)
   - UI control showing connection status with colored indicator

## Features Implemented

### 1. Connection Management

- **Automatic Connection**: Connects when user logs in
- **Reconnection Logic**: Exponential backoff with 5 retry attempts
  - Attempt 1: Immediate
  - Attempt 2: 2 seconds
  - Attempt 3: 5 seconds
  - Attempt 4: 10 seconds
  - Attempt 5: 30 seconds
- **Connection Status Tracking**: Real-time status updates via events

### 2. Real-Time Updates

#### Journal Entries
- New journal entries broadcast to partner immediately
- Automatic UI update when partner creates entry
- Integrated in `JournalViewModel`

#### Social Feed Posts
- New posts broadcast to all followers
- Real-time feed updates without refresh
- Integrated in `SocialFeedViewModel`

#### Mood Tracking
- Mood updates broadcast to partner
- Partner receives notification of mood changes
- Integrated in `MoodTrackerViewModel`

### 3. Event System

The service uses .NET events for loose coupling:

```csharp
event EventHandler<JournalEntryDto>? JournalEntryReceived;
event EventHandler<PostDto>? PostReceived;
event EventHandler<MoodEntryDto>? MoodEntryReceived;
event EventHandler<NotificationDto>? NotificationReceived;
event EventHandler<bool>? ConnectionStatusChanged;
```

## Integration Guide

### Service Registration

The SignalR service is registered as a singleton in `App.xaml.cs`:

```csharp
services.AddSingleton<IRealTimeSyncService, TogetherHub>();
```

### ViewModel Integration

To integrate real-time updates in a ViewModel:

1. **Inject the service**:
```csharp
public MyViewModel(IRealTimeSyncService? realTimeSyncService = null)
{
    _realTimeSyncService = realTimeSyncService;
}
```

2. **Subscribe to events**:
```csharp
if (_realTimeSyncService != null)
{
    _realTimeSyncService.JournalEntryReceived += OnJournalEntryReceived;
}
```

3. **Handle events on UI thread**:
```csharp
private void OnJournalEntryReceived(object? sender, JournalEntryDto entry)
{
    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
    {
        // Update UI
    });
}
```

4. **Dispose properly**:
```csharp
public void Dispose()
{
    if (_realTimeSyncService != null)
    {
        _realTimeSyncService.JournalEntryReceived -= OnJournalEntryReceived;
    }
}
```

### Service Integration

Services broadcast updates after successful operations:

```csharp
// Broadcast to partner
await _realTimeSyncService.BroadcastToPartnerAsync("JournalEntryCreated", entryDto);

// Broadcast to followers
await _realTimeSyncService.BroadcastToFollowersAsync("PostCreated", postDto);
```

## Configuration

### Hub URL

The SignalR hub URL is currently hardcoded in `TogetherHub.cs`:

```csharp
.WithUrl("https://your-signalr-hub-url/togetherhub", options =>
{
    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
})
```

**TODO**: Move this to `appsettings.json` for environment-specific configuration.

### Authentication

The hub uses JWT token authentication:
- Token is provided during connection
- Token is sent with each request via `AccessTokenProvider`

## Server-Side Requirements

The SignalR server must implement the following hub methods:

### Hub Methods (Server)

```csharp
public class TogetherHub : Hub
{
    // Join user-specific group
    Task JoinUserGroup(Guid userId);
    
    // Broadcast to partner
    Task BroadcastToPartner(Guid userId, string eventType, object data);
    
    // Broadcast to followers
    Task BroadcastToFollowers(Guid userId, string eventType, object data);
    
    // Send notification to specific user
    Task NotifyUser(Guid userId, NotificationDto notification);
}
```

### Client Methods (Received by Client)

```csharp
// Receive journal entry
connection.On<JournalEntryDto>("ReceiveJournalEntry", entry => { });

// Receive post
connection.On<PostDto>("ReceivePost", post => { });

// Receive mood entry
connection.On<MoodEntryDto>("ReceiveMoodEntry", mood => { });

// Receive notification
connection.On<NotificationDto>("ReceiveNotification", notification => { });
```

## Error Handling

- **Connection Failures**: Logged and retried automatically
- **Broadcast Failures**: Caught and logged, but don't fail the operation
- **Event Handler Exceptions**: Should be caught in individual handlers

## Testing

### Manual Testing

1. **Connection Status**:
   - Start application
   - Verify connection indicator shows "Connected"
   - Disconnect network
   - Verify indicator shows "Disconnected"
   - Reconnect network
   - Verify automatic reconnection

2. **Real-Time Updates**:
   - Open application on two devices with different users
   - Create journal entry on device 1
   - Verify it appears on device 2 without refresh
   - Repeat for posts and mood entries

### Unit Testing

Mock the `IRealTimeSyncService` in ViewModels:

```csharp
var mockRealTimeService = new Mock<IRealTimeSyncService>();
mockRealTimeService.Setup(s => s.IsConnected).Returns(true);

var viewModel = new JournalViewModel(
    journalService,
    userId,
    connectionId,
    mockRealTimeService.Object
);

// Simulate receiving an entry
mockRealTimeService.Raise(
    s => s.JournalEntryReceived += null,
    mockRealTimeService.Object,
    testEntry
);
```

## Performance Considerations

1. **Connection Pooling**: Single connection per user session
2. **Event Throttling**: Consider throttling high-frequency events
3. **Message Size**: Keep DTOs lightweight
4. **UI Thread**: Always dispatch to UI thread for updates

## Future Enhancements

1. **Typing Indicators**: Show when partner is typing
2. **Read Receipts**: Real-time read status updates
3. **Presence**: Online/offline status for users
4. **Message Queue**: Queue messages when offline
5. **Compression**: Enable message compression for large payloads
6. **Heartbeat**: Custom heartbeat for connection health monitoring

## Troubleshooting

### Connection Issues

- **Check hub URL**: Ensure the URL is correct and accessible
- **Verify authentication**: Check JWT token is valid
- **Network connectivity**: Ensure network allows WebSocket connections
- **Firewall**: Check firewall rules allow SignalR traffic

### Events Not Firing

- **Verify subscription**: Ensure event handlers are subscribed
- **Check server implementation**: Verify server is broadcasting correctly
- **Inspect logs**: Check application logs for errors

### UI Not Updating

- **Dispatcher**: Ensure updates are dispatched to UI thread
- **Event handlers**: Verify event handlers are not throwing exceptions
- **ViewModel lifecycle**: Ensure ViewModel is not disposed prematurely

## Dependencies

- `Microsoft.AspNetCore.SignalR.Client` (v9.0.10)
- `Microsoft.Extensions.Logging.Abstractions` (v9.0.10)

## References

- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [SignalR Client Configuration](https://docs.microsoft.com/en-us/aspnet/core/signalr/dotnet-client)
- [Automatic Reconnection](https://docs.microsoft.com/en-us/aspnet/core/signalr/dotnet-client#automatically-reconnect)
