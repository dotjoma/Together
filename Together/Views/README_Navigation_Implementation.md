# Navigation System Implementation

## Overview

The navigation system has been successfully implemented for the Together application, providing ViewModel-based navigation with history support and parameter passing capabilities.

## Components Implemented

### 1. Navigation Service (`Together/Services/NavigationService.cs`)

The core navigation service that manages ViewModel navigation:

**Features:**
- ViewModel-based navigation using dependency injection
- Navigation history with back button support
- Parameter passing between ViewModels
- Support for `INavigationAware` interface for lifecycle events
- Event-driven architecture for view updates

**Key Methods:**
- `NavigateTo<TViewModel>()` - Navigate to a ViewModel without parameters
- `NavigateTo<TViewModel>(object parameter)` - Navigate with parameters
- `GoBack()` - Navigate to previous ViewModel
- `ClearHistory()` - Clear navigation stack
- `RegisterViewModel<TViewModel>()` - Register ViewModels (for future extensibility)

### 2. Navigation Interface (`Together/Services/INavigationService.cs`)

Defines the contract for navigation operations:
- `CurrentViewModel` property for data binding
- `CurrentViewModelChanged` event for UI updates
- `CanGoBack` property for back button state
- Navigation methods for forward and backward navigation

### 3. Navigation Aware Interface (`Together/Services/INavigationAware.cs`)

Allows ViewModels to respond to navigation events:
- `OnNavigatedTo(object? parameter)` - Called when navigating to the ViewModel
- `OnNavigatedFrom()` - Called when navigating away from the ViewModel

### 4. Main Window (`Together/MainWindow.xaml`)

Material Design-based main window with:

**UI Components:**
- Top app bar with menu button, back button, and user info
- Navigation drawer with categorized menu items:
  - **Couple Features**: Couple Hub, Shared Journal, Mood Tracker, To-Do List, Events & Calendar, Challenges, Virtual Pet, Long Distance
  - **Social Features**: Social Feed, My Profile
- Content area with automatic View-ViewModel mapping using DataTemplates
- User profile section in drawer showing avatar, username, and email

**Navigation Features:**
- Hamburger menu to toggle navigation drawer
- Back button for navigation history
- Logout functionality
- Visual feedback for current user

### 5. Main ViewModel (`Together/ViewModels/MainViewModel.cs`)

Coordinates the entire application navigation:

**Responsibilities:**
- Manages current ViewModel display
- Handles navigation commands for all modules
- Maintains user session state
- Controls navigation drawer state
- Implements logout functionality

**Navigation Commands:**
- `NavigateToCoupleHubCommand`
- `NavigateToJournalCommand`
- `NavigateToMoodCommand`
- `NavigateToSocialFeedCommand`
- `NavigateToProfileCommand`
- `NavigateToCalendarCommand`
- `NavigateToTodoCommand`
- `NavigateToChallengesCommand`
- `NavigateToVirtualPetCommand`
- `NavigateToLongDistanceCommand`
- `GoBackCommand`
- `LogoutCommand`

### 6. Updated ViewModels

All main ViewModels have been updated to implement `INavigationAware`:

**Updated ViewModels:**
- `CoupleHubViewModel` - Dashboard view
- `JournalViewModel` - Shared journal
- `MoodTrackerViewModel` - Mood tracking
- `SocialFeedViewModel` - Social feed
- `UserProfileViewModel` - User profile
- `CalendarViewModel` - Events and calendar
- `TodoListViewModel` - To-do list
- `ChallengeViewModel` - Challenges
- `VirtualPetViewModel` - Virtual pet
- `LongDistanceViewModel` - Long distance features

**Changes Made:**
- Removed Guid parameters from constructors
- Implemented `INavigationAware` interface
- Get current user from `Application.Properties["CurrentUser"]`
- Load couple connection data when needed
- Initialize data in `OnNavigatedTo` method

### 7. Dependency Injection Registration (`Together/App.xaml.cs`)

All services and ViewModels are registered in the DI container:

**Services:**
- `INavigationService` registered as Singleton
- All application services registered as Scoped
- All ViewModels registered as Transient

**Startup Flow:**
1. Application starts with LoginView
2. After successful login, MainWindow is shown
3. MainViewModel initializes with current user
4. Default navigation to CoupleHubViewModel

## Usage Examples

### Basic Navigation

```csharp
// Navigate to a view
_navigationService.NavigateTo<SocialFeedViewModel>();

// Navigate with parameter
_navigationService.NavigateTo<UserProfileViewModel>(userId);

// Go back
_navigationService.GoBack();
```

### Implementing INavigationAware

```csharp
public class MyViewModel : ViewModelBase, INavigationAware
{
    public void OnNavigatedTo(object? parameter)
    {
        // Initialize data when navigating to this view
        if (parameter is Guid id)
        {
            // Use the parameter
        }
        
        // Get current user
        var currentUser = Application.Current.Properties["CurrentUser"] as UserDto;
    }
    
    public void OnNavigatedFrom()
    {
        // Cleanup when leaving this view
    }
}
```

### View-ViewModel Mapping

In MainWindow.xaml, DataTemplates automatically map ViewModels to Views:

```xaml
<ContentControl Content="{Binding CurrentViewModel}">
    <ContentControl.Resources>
        <DataTemplate DataType="{x:Type viewmodels:CoupleHubViewModel}">
            <local:CoupleHubView/>
        </DataTemplate>
        <!-- More mappings... -->
    </ContentControl.Resources>
</ContentControl>
```

## Navigation Flow

1. User clicks navigation menu item
2. MainViewModel executes navigation command
3. NavigationService creates new ViewModel instance via DI
4. If ViewModel implements INavigationAware, `OnNavigatedTo` is called
5. Current ViewModel is updated
6. `CurrentViewModelChanged` event fires
7. MainWindow's ContentControl updates via data binding
8. Appropriate View is displayed based on DataTemplate

## Benefits

1. **Decoupled Architecture**: Views and ViewModels are loosely coupled
2. **Testability**: ViewModels can be tested without UI
3. **Flexibility**: Easy to add new views and navigation paths
4. **History Support**: Built-in back button functionality
5. **Parameter Passing**: Type-safe parameter passing between views
6. **Lifecycle Management**: ViewModels can respond to navigation events
7. **Dependency Injection**: All dependencies are injected, improving maintainability

## Future Enhancements

1. **Deep Linking**: Support for URL-based navigation
2. **Navigation Guards**: Prevent navigation based on conditions
3. **Breadcrumb Navigation**: Show navigation path
4. **Tab Navigation**: Support for tabbed interfaces
5. **Modal Navigation**: Support for dialog-based navigation
6. **Navigation Analytics**: Track user navigation patterns

## Testing

The navigation system can be tested by:

1. **Unit Testing NavigationService**: Mock IServiceProvider and test navigation logic
2. **Unit Testing ViewModels**: Test INavigationAware implementations
3. **Integration Testing**: Test complete navigation flows
4. **UI Testing**: Test user interactions with navigation elements

## Notes

- All ViewModels must be registered in the DI container
- Views must have corresponding DataTemplates in MainWindow.xaml
- Current user is stored in `Application.Properties["CurrentUser"]`
- Navigation history is maintained automatically
- Back button is disabled when history is empty

## Compliance

This implementation satisfies **Requirement 20.1** from the requirements document:
- ViewModel-based navigation ✓
- Navigation history with back button ✓
- Module registration system ✓
- Parameter passing between views ✓
- Main window with navigation drawer ✓
- Navigation menu items for all modules ✓
- Current user information display ✓
- Logout functionality ✓
