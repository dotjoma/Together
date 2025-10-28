# Love Streak Implementation

## Overview
The Love Streak feature tracks consecutive days of interaction between partners, encouraging daily engagement and celebrating milestones.

## Components

### 1. Service Layer
**LoveStreakService** (`Together.Application/Services/LoveStreakService.cs`)
- Tracks daily interactions (journal entries, mood logs, todo completions, challenge completions)
- Increments streak for consecutive day interactions
- Resets streak after 24 hours of inactivity
- Detects and celebrates milestones (7, 30, 100, 365 days)
- Sends notifications for milestone achievements and streak resets

### 2. ViewModels

#### StreakWidgetViewModel
**Location**: `Together/ViewModels/StreakWidgetViewModel.cs`

**Purpose**: Manages the display of current streak, progress, and milestones

**Key Properties**:
- `CurrentStreak`: Current streak count in days
- `StreakDisplay`: Formatted display string (e.g., "7 Days")
- `NextMilestone`: Next milestone to achieve
- `ProgressToNextMilestone`: Progress percentage to next milestone
- `AchievedMilestones`: List of achieved milestone values
- `StreakMessage`: Motivational message based on current streak
- `ShowCelebration`: Triggers celebration animation

**Key Methods**:
- `LoadStreakDataAsync()`: Loads current streak and milestone data
- `RefreshStreakAsync()`: Refreshes streak data
- `TriggerCelebration(int milestone)`: Shows celebration for milestone achievement

#### StreakHistoryViewModel
**Location**: `Together/ViewModels/StreakHistoryViewModel.cs`

**Purpose**: Displays streak widget and interaction history

**Key Properties**:
- `StreakWidget`: Instance of StreakWidgetViewModel
- `InteractionHistory`: Collection of recent interactions
- `IsLoading`: Loading state indicator

**Key Methods**:
- `InitializeAsync()`: Initializes the view with connection data
- `RefreshAsync()`: Refreshes streak and history data
- `LoadInteractionHistoryAsync()`: Loads recent interaction history

### 3. Views and Controls

#### StreakWidget
**Location**: `Together/Controls/StreakWidget.xaml`

**Features**:
- Prominent display of current streak count
- Fire icon to represent the streak
- Motivational message
- Progress bar showing progress to next milestone
- Display of achieved milestones as chips
- Celebration overlay with animation for milestone achievements
- Loading indicator

**Visual Design**:
- Orange/red color scheme (#FF6B35) for fire/streak theme
- Material Design card layout
- Smooth animations for celebration
- Trophy icons for achieved milestones

#### StreakHistoryView
**Location**: `Together/Views/StreakHistoryView.xaml`

**Features**:
- Displays StreakWidget at the top
- Shows recent interaction history with:
  - Interaction type icons
  - Color-coded interaction types
  - Relative timestamps (e.g., "2h ago")
  - Interaction descriptions
- Refresh button to update data
- Empty state when no interactions exist
- Loading indicator

### 4. Converters

#### NotZeroToVisibilityConverter
**Location**: `Together/Converters/NotZeroToVisibilityConverter.cs`

**Purpose**: Shows elements when value is greater than zero

**Usage**: Used to show milestone progress and achieved milestones sections

#### NullToVisibilityConverter
**Location**: `Together/Converters/NullToVisibilityConverter.cs`

**Purpose**: Shows/hides elements based on null state

**Parameters**: 
- No parameter: Visible when not null
- "Inverse": Visible when null

## Interaction Types

The system tracks the following interaction types:
- **JournalEntry**: Shared journal entries
- **MoodLog**: Mood tracking entries
- **ChatMessage**: Chat messages (future implementation)
- **ChallengeCompletion**: Completed challenges
- **TodoCompletion**: Completed todo items

Each interaction type has:
- Unique icon (BookOpen, EmoticonHappy, Message, Trophy, CheckCircle)
- Color coding for visual distinction
- Contribution to daily streak counter

## Streak Logic

### Streak Increment Rules
1. **First interaction of the day**: 
   - If last interaction was yesterday (exactly 1 day ago), increment streak
   - If last interaction was more than 1 day ago, reset to 1
   - If no previous interaction, start at 1

2. **Same-day interactions**: 
   - Update timestamp but don't increment streak
   - Multiple interactions per day count as one streak day

3. **Streak reset**:
   - Automatically resets if no interaction for more than 24 hours
   - Both partners receive notification about streak loss

### Milestones
- **7 days**: "🎉 Amazing! You've reached a 7-day love streak!"
- **30 days**: "🌟 Incredible! 30 days of staying connected!"
- **100 days**: "💯 Wow! 100 days of love and dedication!"
- **365 days**: "🏆 Legendary! A full year of daily connection!"

### Notifications
- Milestone achievements trigger notifications for both partners
- Streak resets trigger motivational notifications
- Notifications include celebration emojis and encouraging messages

## Usage Example

### In a View (e.g., Couple Hub Dashboard)

```xaml
<controls:StreakWidget DataContext="{Binding StreakWidget}"/>
```

### In a ViewModel

```csharp
public class CoupleHubViewModel : ViewModelBase
{
    private readonly ILoveStreakService _loveStreakService;
    private StreakWidgetViewModel _streakWidget;
    
    public CoupleHubViewModel(ILoveStreakService loveStreakService)
    {
        _loveStreakService = loveStreakService;
        // Initialize with connection ID
        _streakWidget = new StreakWidgetViewModel(_loveStreakService, connectionId);
    }
    
    public StreakWidgetViewModel StreakWidget
    {
        get => _streakWidget;
        set => SetProperty(ref _streakWidget, value);
    }
}
```

### Recording an Interaction

```csharp
// When a journal entry is created
await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.JournalEntry);

// When a mood is logged
await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.MoodLog);

// When a todo is completed
await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.TodoCompletion);
```

## Integration Points

### Services to Update
When implementing other features, ensure they record interactions:

1. **JournalService.CreateJournalEntryAsync**
   - Add: `await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.JournalEntry);`

2. **MoodTrackingService.CreateMoodEntryAsync**
   - Add: `await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.MoodLog);`

3. **TodoService.MarkAsCompleteAsync**
   - Add: `await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.TodoCompletion);`

4. **ChallengeService.CompleteChallengeAsync**
   - Add: `await _loveStreakService.RecordInteractionAsync(connectionId, InteractionType.ChallengeCompletion);`

### Background Task
Consider implementing a background task that runs daily to:
- Check all active connections for streak resets
- Call `CheckAndResetStreakAsync` for connections with no recent activity

## Testing Considerations

### Unit Tests
- Test streak increment logic for consecutive days
- Test streak reset after 24 hours
- Test milestone detection
- Test notification generation

### Integration Tests
- Test interaction recording from different services
- Test streak persistence across application restarts
- Test notification delivery to both partners

### UI Tests
- Verify streak display updates correctly
- Test celebration animation triggers
- Verify milestone chips display
- Test progress bar calculation

## Future Enhancements

1. **Streak History Graph**: Line chart showing streak over time
2. **Streak Freeze**: Allow partners to "freeze" streak for planned absences
3. **Streak Leaderboard**: Compare with other couples (opt-in)
4. **Custom Milestones**: Allow couples to set personal milestone goals
5. **Streak Rewards**: Unlock virtual pet features or themes based on streaks
6. **Streak Recovery**: One-time recovery option if streak is lost accidentally
7. **Weekly/Monthly Stats**: Aggregate statistics on interaction patterns

## Dependencies

- `Together.Application.Interfaces.ILoveStreakService`
- `Together.Application.Interfaces.ICoupleConnectionService`
- `Together.Domain.Enums.InteractionType`
- `MaterialDesignThemes.Wpf` for UI components
- `Together.Presentation.Commands.RelayCommand` for command binding

## Registration

The service is registered in `App.xaml.cs`:

```csharp
services.AddScoped<ILoveStreakService, LoveStreakService>();
```

Converters are registered in `App.xaml`:

```xaml
<converters:NotZeroToVisibilityConverter x:Key="NotZeroToVisibilityConverter"/>
<converters:NullToVisibilityConverter x:Key="NullToVisibilityConverter"/>
```
