# UserProfileView Documentation

## Overview
The UserProfileView displays a user's profile information including username, email, bio, profile picture, and follower/following counts. It supports editing mode where users can update their profile information.

## Features Implemented

### Display Mode
- **Profile Picture**: Displays user's profile picture with a circular border. Shows a default account icon if no picture is set.
- **Username and Email**: Prominently displayed at the top of the profile.
- **Follower/Following Counts**: Shows the number of followers and users being followed.
- **Bio**: Displays user's bio text. Shows "No bio added yet" if empty.
- **Visibility Setting**: Shows current profile visibility (Public, Friends Only, or Private).

### Edit Mode
- **Edit Profile Button**: Switches to edit mode when clicked.
- **Bio Editor**: Multi-line text box with 500 character limit for editing bio.
- **Visibility Toggle**: Radio buttons to select profile visibility:
  - Public - Anyone can see the profile
  - Friends Only - Only followers can see the profile
  - Private - Only the user can see the profile
- **Profile Picture Upload**: 
  - Button to select a new profile picture
  - Supports JPG and PNG formats
  - Maximum file size: 2MB
  - Shows selected file name before saving
- **Save/Cancel Buttons**: Save changes or cancel and revert to original values.

## Usage

### Creating the ViewModel
```csharp
var profileService = serviceProvider.GetRequiredService<IProfileService>();
var userId = Guid.Parse("user-id-here");
var viewModel = new UserProfileViewModel(profileService, userId);
```

### Setting the DataContext
```csharp
var view = new UserProfileView();
view.DataContext = viewModel;
```

## Requirements Satisfied
- **Requirement 11.2**: Profile update methods (bio, profile picture, visibility settings)
- **Requirement 11.3**: Profile picture uploads with 2MB limit
- **Requirement 11.4**: Visibility toggle (public, friends-only, private)

## Dependencies
- **IProfileService**: For loading and updating profile data
- **Material Design in XAML**: For UI styling and components
- **Converters**:
  - BooleanToVisibilityConverter: Shows/hides elements based on boolean values
  - InverseBooleanConverter: Inverts boolean values
  - StringToVisibilityConverter: Shows elements only when string has value
  - EnumToBooleanConverter: Binds enum values to radio buttons

## Error Handling
- Loading errors are displayed in a red card at the top
- Validation errors (file size, format) are shown inline
- All async operations show a loading indicator

## Future Enhancements
- Add follower/following list views
- Add ability to view other users' profiles
- Add profile statistics and activity history
