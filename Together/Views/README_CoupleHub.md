# Couple Hub Dashboard Implementation

## Overview
The Couple Hub Dashboard is the main dashboard view that aggregates and displays key relationship information, partner status, and activity summaries for connected couples.

## Components Implemented

### 1. DTOs (Data Transfer Objects)

#### DashboardSummaryDto
- **Location**: `Together.Application/DTOs/DashboardSummaryDto.cs`
- **Purpose**: Aggregates all dashboard data in a single response
- **Properties**:
  - `PartnerMood`: Latest mood entry from partner
  - `LoveStreak`: Current consecutive days of interaction
  - `UpcomingEvents`: Next 5 upcoming shared events
  - `VirtualPet`: Current virtual pet status
  - `DaysTogether`: Total days since relationship start
  - `SupportiveMessage`: Generated message when partner has negative mood

#### TogetherMomentDto
- **Location**: `Together.Application/DTOs/TogetherMomentDto.cs`
- **Purpose**: Represents a single activity or moment in the relationship
- **Properties**:
  - `Id`: Unique identifier
  - `ActivityType`: Type of activity (JournalEntry, MoodUpdate, TodoCompleted)
  - `Description`: Human-readable description
  - `User`: User who performed the activity
  - `Timestamp`: When the activity occurred

#### DailySuggestionDto
- **Location**: `Together.Application/DTOs/DailySuggestionDto.cs`
- **Purpose**: Provides daily activity or conversation suggestions
- **Properties**:
  - `Category`: Suggestion category (Fun, Appreciation, Communication, Learning)
  - `Title`: Suggestion title
  - `Description`: Detailed suggestion text

### 2. Service Layer

#### IDashboardService Interface
- **Location**: `Together.Application/Interfaces/IDashboardService.cs`
- **Methods**:
  - `GetDashboardSummaryAsync(Guid userId)`: Aggregates all dashboard data
  - `GetTogetherMomentsAsync(Guid userId, int limit)`: Returns recent activities
  - `GetDailySuggestionAsync(Guid userId)`: Generates daily suggestion

#### DashboardService Implementation
- **Location**: `Together.Application/Services/DashboardService.cs`
- **Dependencies**:
  - `ICoupleConnectionRepository`: Access couple connection data
  - `IMoodTrackingService`: Get partner mood information
  - `ILoveStreakService`: Retrieve current streak
  - `IEventService`: Get upcoming events and days together
  - `IVirtualPetService`: Fetch virtual pet status
  - `IJournalService`: Access journal entries
  - `ITodoService`: Get todo items
  - `IMoodAnalysisService`: Generate supportive messages
  - Various repositories for direct data access

**Key Features**:
- **Dashboard Summary Aggregation**: Combines data from multiple services
- **Together Moments Feed**: Aggregates recent activities from:
  - Journal entries (last 7 days)
  - Mood updates (last 7 days)
  - Completed todos
- **Daily Suggestions**: 
  - 15 positive activity suggestions across 4 categories
  - 15 conversation starters
  - Deterministic selection based on user ID and date (same suggestion all day)
- **Supportive Message Detection**: Automatically detects negative partner moods and generates supportive messages

### 3. Presentation Layer

#### CoupleHubViewModel
- **Location**: `Together/ViewModels/CoupleHubViewModel.cs`
- **Properties**:
  - `PartnerMood`: Bindable partner mood data
  - `LoveStreak`: Current streak count
  - `VirtualPet`: Pet status and level
  - `DaysTogether`: Relationship duration
  - `SupportiveMessage`: Message to show when partner needs support
  - `DailySuggestion`: Today's activity suggestion
  - `UpcomingEvents`: Observable collection of upcoming events
  - `TogetherMoments`: Observable collection of recent activities
  - `IsLoading`: Loading state indicator
  - `ErrorMessage`: Error display

- **Commands**:
  - `RefreshCommand`: Reloads all dashboard data

- **Behavior**:
  - Automatically loads data on initialization
  - Handles errors gracefully with user-friendly messages
  - Updates UI through property change notifications

#### CoupleHubView
- **Location**: `Together/Views/CoupleHubView.xaml`
- **Layout**: Responsive grid-based dashboard with multiple widgets

**UI Sections**:

1. **Header**
   - Dashboard title
   - Days together counter
   - Refresh button

2. **Supportive Message Card** (conditional)
   - Displays when partner has negative mood
   - Warm orange background (#FFF3E0)
   - Heart icon and supportive message text
   - Only visible when `SupportiveMessage` is not null

3. **Main Dashboard Widgets** (3-column grid)
   - **Partner Mood Widget**:
     - Shows partner's latest mood
     - Displays mood type, timestamp, and notes
     - Shows "No mood logged yet" when null
   
   - **Love Streak Widget**:
     - Large display of current streak count
     - Fire icon in orange (#FF6B35)
     - "days in a row" subtitle
   
   - **Virtual Pet Widget**:
     - Pet name and level
     - Current state (Happy, Sad, etc.)
     - XP progress bar
     - Shows "No pet yet" when null

4. **Daily Suggestion Card**
   - Green background (#E8F5E9)
   - Lightbulb icon
   - Suggestion title and description
   - Category chip

5. **Bottom Section** (2-column grid)
   - **Upcoming Events**:
     - Calendar icon
     - List of next 5 events
     - Event title and date/time
     - Shows "No upcoming events" when empty
   
   - **Together Moments**:
     - Heart icon
     - Feed of last 5 activities
     - Activity description and timestamp
     - Shows "No recent moments" when empty

6. **Loading Overlay**
   - Semi-transparent white background
   - Circular progress indicator
   - "Loading dashboard..." text

7. **Error Display**
   - Red card with error icon
   - Error message
   - Retry button

**Material Design Elements**:
- Cards for all sections
- PackIcons for visual indicators
- Consistent color scheme:
  - Green (#4CAF50) for positive/mood
  - Orange (#FF6B35) for streak
  - Purple (#9C27B0) for pet
  - Blue (#2196F3) for events
  - Pink (#E91E63) for moments
- Proper spacing and margins
- Responsive layout

### 4. Dependency Injection

#### Registration
- **Location**: `Together/App.xaml.cs`
- **Service**: `services.AddScoped<IDashboardService, DashboardService>()`
- **Scope**: Scoped (per request/operation)

## Requirements Mapping

This implementation satisfies the following requirements from the specification:

### Requirement 17.1
✅ Dashboard displays current day summary including partner mood, Love Streak, and upcoming events

### Requirement 17.2
✅ Virtual Pet status and current level displayed prominently on dashboard

### Requirement 17.3
✅ Shows most recent Together Moments (last 5 activities) from both Partners

### Requirement 17.4
✅ Displays supportive message suggestion when Partner's mood indicates negative emotions

### Requirement 17.5
✅ Generates and displays random daily positive activity or conversation starter

## Usage Example

```csharp
// In a ViewModel or View code-behind
var dashboardService = serviceProvider.GetRequiredService<IDashboardService>();
var currentUserId = GetCurrentUserId();

// Create ViewModel
var viewModel = new CoupleHubViewModel(dashboardService, currentUserId);

// Set as DataContext
var view = new CoupleHubView { DataContext = viewModel };
```

## Data Flow

1. **Initialization**:
   - `CoupleHubViewModel` constructor calls `LoadDashboardDataAsync()`
   - Sets `IsLoading = true`

2. **Data Loading**:
   - Calls `DashboardService.GetDashboardSummaryAsync()`
   - Service aggregates data from multiple sources:
     - Partner mood from `MoodTrackingService`
     - Love streak from `LoveStreakService`
     - Events from `EventService`
     - Virtual pet from `VirtualPetService`
     - Days together calculated from relationship start date
   - Generates supportive message if partner mood is negative

3. **Together Moments**:
   - Calls `DashboardService.GetTogetherMomentsAsync()`
   - Aggregates from repositories:
     - Recent journal entries
     - Recent mood updates
     - Completed todos
   - Sorts by timestamp and returns top 5

4. **Daily Suggestion**:
   - Calls `DashboardService.GetDailySuggestionAsync()`
   - Uses deterministic random selection based on user ID + date
   - Returns either positive activity or conversation starter

5. **UI Update**:
   - ViewModel properties updated via `SetProperty()`
   - `PropertyChanged` events trigger UI updates
   - Observable collections automatically update ItemsControls
   - Sets `IsLoading = false`

6. **Error Handling**:
   - Catches exceptions during data loading
   - Sets `ErrorMessage` property
   - UI displays error card with retry option

## Testing Considerations

### Unit Tests
- Test `DashboardService.GetDashboardSummaryAsync()` with various scenarios:
  - Partner with positive mood
  - Partner with negative mood (verify supportive message)
  - No partner mood logged
  - No virtual pet
  - No upcoming events
- Test `GetTogetherMomentsAsync()` aggregation logic
- Test `GetDailySuggestionAsync()` deterministic selection

### Integration Tests
- Test full data flow from repositories through service to ViewModel
- Verify proper error handling when services fail
- Test refresh functionality

### UI Tests
- Verify all widgets display correctly
- Test conditional visibility (supportive message, empty states)
- Verify loading and error states
- Test refresh command

## Future Enhancements

1. **Real-time Updates**:
   - Integrate SignalR to push updates when partner logs mood or completes activities
   - Auto-refresh dashboard when new data arrives

2. **Customization**:
   - Allow users to choose which widgets to display
   - Reorderable dashboard layout
   - Widget size preferences

3. **Analytics**:
   - Track which suggestions users act on
   - Mood correlation analysis
   - Streak prediction

4. **Notifications**:
   - Push notifications for important moments
   - Reminder to check dashboard daily
   - Streak warning when approaching reset

5. **Animations**:
   - Smooth transitions when data updates
   - Celebration animations for milestones
   - Pet animations based on state

## Notes

- The dashboard automatically loads on initialization
- All data is fetched asynchronously to prevent UI blocking
- Error handling provides user-friendly messages
- The daily suggestion is consistent throughout the day (same seed)
- Together moments are limited to last 7 days to keep feed relevant
- All timestamps are displayed in local time format
- The supportive message only appears when partner has negative mood
