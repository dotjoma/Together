# Follow Service Implementation

## Overview
The Follow Service manages the social follow system, allowing users to send follow requests, accept/reject requests, and manage their follower/following relationships.

## Features Implemented

### Service Methods
- **SendFollowRequestAsync**: Creates a pending follow relationship between two users
- **AcceptFollowRequestAsync**: Accepts a pending follow request and establishes the relationship
- **RejectFollowRequestAsync**: Rejects and deletes a pending follow request
- **UnfollowAsync**: Removes an existing follow relationship
- **GetFollowersAsync**: Retrieves all users following a specific user
- **GetFollowingAsync**: Retrieves all users that a specific user is following
- **GetPendingRequestsAsync**: Retrieves all pending follow requests for a user
- **GetFollowerCountAsync**: Gets the count of followers for a user
- **GetFollowingCountAsync**: Gets the count of users being followed
- **GetFollowStatusAsync**: Determines the follow status between two users (none, pending, accepted, self)

## Validation Rules

### Follow Request Validation
- Users cannot follow themselves
- Cannot send duplicate follow requests
- Cannot follow a user already being followed
- Both users must exist in the system

### Request Processing
- Only pending requests can be accepted or rejected
- Rejected requests are deleted from the system
- Accepted requests update the status and timestamp

## Data Flow

1. **Sending a Follow Request**
   - Validate both users exist
   - Check for existing relationships
   - Create new FollowRelationship with "pending" status
   - Save to database

2. **Accepting a Request**
   - Validate request exists and is pending
   - Update status to "accepted"
   - Set AcceptedAt timestamp
   - Save changes

3. **Rejecting a Request**
   - Validate request exists and is pending
   - Delete the relationship record

4. **Unfollowing**
   - Find existing relationship
   - Delete the relationship record

## Integration Points

### Dependencies
- **IFollowRelationshipRepository**: Data access for follow relationships
- **IUserRepository**: User validation and retrieval

### Used By
- **ProfileService**: Gets follower/following counts for user profiles
- **FollowRequestsViewModel**: Manages pending follow requests UI
- **FollowerListViewModel**: Displays user's followers
- **FollowingListViewModel**: Displays users being followed
- **FollowButton**: Shows follow status and handles follow actions

## Error Handling

### Exceptions Thrown
- **NotFoundException**: When user or relationship is not found
- **ValidationException**: When business rules are violated (duplicate requests, invalid status changes)

### Error Scenarios
- User not found
- Relationship already exists
- Request not pending
- Self-follow attempt

## Usage Example

```csharp
// Send a follow request
var request = await _followService.SendFollowRequestAsync(currentUserId, targetUserId);

// Accept a follow request
var accepted = await _followService.AcceptFollowRequestAsync(requestId);

// Get followers
var followers = await _followService.GetFollowersAsync(userId);

// Check follow status
var status = await _followService.GetFollowStatusAsync(currentUserId, targetUserId);
// Returns: "none", "pending", "accepted", or "self"
```

## Requirements Satisfied
- Requirement 12.1: Send follow requests creating pending relationships
- Requirement 12.2: Notify and display follow requests for approval
- Requirement 12.3: Accept follow requests establishing one-way relationships
- Requirement 12.5: Unfollow functionality removing relationships
