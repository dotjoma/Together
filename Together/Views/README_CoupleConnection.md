# Couple Connection System

## Overview
The couple connection system allows users to establish a mutual connection with a partner, enabling access to private shared features like journals, mood tracking, and relationship goals.

## Components

### Domain Layer
- **ConnectionRequest Entity**: Represents a pending, accepted, or rejected connection request
- **CoupleConnection Entity**: Represents an active couple connection with relationship data
- **Repositories**: 
  - `IConnectionRequestRepository`: Manages connection request persistence
  - `ICoupleConnectionRepository`: Manages couple connection persistence

### Application Layer
- **CoupleConnectionService**: Core business logic for connection management
  - `SendConnectionRequestAsync`: Creates a new connection request
  - `AcceptConnectionRequestAsync`: Accepts a pending request and creates a connection
  - `RejectConnectionRequestAsync`: Rejects a pending request
  - `TerminateConnectionAsync`: Ends an active connection and archives shared data
  - `GetUserConnectionAsync`: Retrieves the user's current connection
  - `GetPendingRequestsAsync`: Gets all pending requests for a user

### Presentation Layer

#### ViewModels
1. **ConnectionRequestViewModel**: Handles sending connection requests
   - Search for users by username or email
   - Send connection requests
   - Display success/error messages

2. **ConnectionRequestNotificationViewModel**: Manages incoming connection requests
   - Display pending requests
   - Accept/reject requests with confirmation dialogs
   - Auto-refresh on load

3. **ConnectionStatusViewModel**: Shows current connection status
   - Display partner information
   - Show relationship statistics (days together, love streak)
   - Terminate connection with confirmation

#### Views
1. **ConnectionRequestView**: UI for sending connection requests
2. **ConnectionRequestNotificationView**: UI for viewing and responding to requests
3. **ConnectionStatusView**: UI for viewing current connection status

## Business Rules

### One Connection Per User
- Each user can only have one active couple connection at a time
- Attempting to send a request while having an active connection will fail
- Attempting to accept a request while having an active connection will fail

### Request Validation
- Users cannot send connection requests to themselves
- Users cannot send duplicate requests to the same person
- If User A sends a request to User B, User B must respond before User A can send another

### Connection Termination
- Only users who are part of the connection can terminate it
- Terminating a connection changes its status to "Terminated"
- Shared data is archived but not deleted

## Usage

### Sending a Connection Request
```csharp
var service = serviceProvider.GetRequiredService<ICoupleConnectionService>();
var request = await service.SendConnectionRequestAsync(currentUserId, targetUserId);
```

### Accepting a Request
```csharp
var connection = await service.AcceptConnectionRequestAsync(requestId, currentUserId);
```

### Checking Connection Status
```csharp
var connection = await service.GetUserConnectionAsync(currentUserId);
if (connection != null)
{
    // User has an active connection
}
```

## Error Handling
- `NotFoundException`: Thrown when a user or request is not found
- `BusinessRuleViolationException`: Thrown when business rules are violated (e.g., already has connection)
- `ValidationException`: Thrown for invalid input data

## Future Enhancements
- Relationship start date customization during request acceptance
- Connection request expiration (auto-reject after X days)
- Block list to prevent unwanted requests
- Connection history and statistics
- Reconnection with previous partners
