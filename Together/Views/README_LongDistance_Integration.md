# Long Distance Features - Integration Guide

## Overview
This guide provides step-by-step instructions for integrating the long-distance features into the Together application.

## Prerequisites
- All long-distance feature files have been created
- Services are registered in App.xaml.cs
- Build completes successfully

## Step 1: Database Migration

Create and apply a migration for the new database columns:

```bash
# Navigate to the Infrastructure project
cd Together.Infrastructure

# Create migration
dotnet ef migrations add AddLongDistanceFeatures --startup-project ../Together/Together.csproj

# Apply migration
dotnet ef database update --startup-project ../Together/Together.csproj
```

### Migration Details
The migration will add the following columns:

**users table:**
- `latitude` (double, nullable)
- `longitude` (double, nullable)
- `timezone_id` (varchar(100), nullable)

**couple_connections table:**
- `next_meeting_date` (timestamp, nullable)

## Step 2: Navigation Integration

### Option A: Add to Main Navigation Menu

If you have a navigation menu in MainWindow.xaml, add:

```xml
<Button Command="{Binding NavigateToLongDistanceCommand}">
    <StackPanel Orientation="Horizontal">
        <materialDesign:PackIcon Kind="MapMarkerDistance" 
                               VerticalAlignment="Center"
                               Margin="0,0,8,0"/>
        <TextBlock Text="Long Distance" 
                  VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

In MainViewModel.cs:

```csharp
public ICommand NavigateToLongDistanceCommand { get; }

// In constructor:
NavigateToLongDistanceCommand = new RelayCommand(_ => NavigateToLongDistance());

private void NavigateToLongDistance()
{
    var viewModel = new LongDistanceViewModel(
        _longDistanceService, 
        _currentConnectionId, 
        _currentUserId);
    CurrentViewModel = viewModel;
}
```

### Option B: Add to Couple Hub Dashboard

In CoupleHubView.xaml, add the DistanceWidget:

```xml
<controls:DistanceWidget DataContext="{Binding DistanceWidgetViewModel}"/>
```

In CoupleHubViewModel.cs:

```csharp
private DistanceWidgetViewModel? _distanceWidgetViewModel;

public DistanceWidgetViewModel? DistanceWidgetViewModel
{
    get => _distanceWidgetViewModel;
    set => SetProperty(ref _distanceWidgetViewModel, value);
}

// In constructor or initialization:
if (_connectionId != Guid.Empty)
{
    DistanceWidgetViewModel = new DistanceWidgetViewModel(
        _longDistanceService, 
        _connectionId);
}
```

## Step 3: User Profile Integration

Add location settings to the user profile view:

In UserProfileView.xaml:

```xml
<Button Style="{StaticResource MaterialDesignOutlinedButton}"
       Command="{Binding OpenLocationSettingsCommand}"
       Margin="0,8,0,0">
    <StackPanel Orientation="Horizontal">
        <materialDesign:PackIcon Kind="MapMarker" 
                               VerticalAlignment="Center"
                               Margin="0,0,8,0"/>
        <TextBlock Text="Location Settings" 
                  VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

In UserProfileViewModel.cs:

```csharp
public ICommand OpenLocationSettingsCommand { get; }

// In constructor:
OpenLocationSettingsCommand = new RelayCommand(_ => OpenLocationSettings());

private void OpenLocationSettings()
{
    var viewModel = new LocationSettingsViewModel(
        _longDistanceService, 
        _currentUserId);
    // Show in dialog or navigate
    _navigationService.NavigateTo(viewModel);
}
```

## Step 4: Testing

### Manual Testing Checklist

1. **Location Settings**
   - [ ] Open location settings view
   - [ ] Enter valid coordinates (e.g., 40.7128, -74.0060)
   - [ ] Select a timezone
   - [ ] Click Save
   - [ ] Verify success message
   - [ ] Try invalid coordinates (e.g., 100, 200)
   - [ ] Verify error message

2. **Distance Display**
   - [ ] Have both users set their location
   - [ ] Open long-distance view
   - [ ] Verify distance is displayed
   - [ ] Check both km and miles are calculated
   - [ ] Verify distance is reasonable

3. **Timezone Display**
   - [ ] Set different timezones for both users
   - [ ] Verify both local times are shown
   - [ ] Verify timezone labels are correct
   - [ ] Check time difference is accurate

4. **Communication Window**
   - [ ] Set timezones with overlap
   - [ ] Verify optimal window is displayed
   - [ ] Set timezones with no overlap (e.g., UTC and UTC+12)
   - [ ] Verify graceful handling

5. **Next Meeting Date**
   - [ ] Click "Set Next Meeting Date"
   - [ ] Select a future date and time
   - [ ] Click Save
   - [ ] Verify countdown timer appears
   - [ ] Check days, hours, minutes, seconds
   - [ ] Wait and verify timer updates

6. **Edge Cases**
   - [ ] Test with no location set
   - [ ] Test with only one user having location
   - [ ] Test with past meeting date (should reject)
   - [ ] Test clearing location
   - [ ] Test with same timezone
   - [ ] Test with maximum time difference

## Step 5: Performance Optimization

### Timer Management

The DistanceWidget uses a DispatcherTimer that updates every second. Ensure proper disposal:

```csharp
// In the view or parent ViewModel
protected override void OnDispose()
{
    DistanceWidgetViewModel?.Dispose();
    base.OnDispose();
}
```

### Caching

Consider caching long-distance info to reduce database calls:

```csharp
// In LongDistanceService
private readonly IMemoryCache _cache;
private const string CacheKeyPrefix = "LongDistance_";

public async Task<LongDistanceInfoDto> GetLongDistanceInfoAsync(Guid connectionId)
{
    var cacheKey = $"{CacheKeyPrefix}{connectionId}";
    
    if (_cache.TryGetValue(cacheKey, out LongDistanceInfoDto? cached))
        return cached!;
    
    var info = await LoadLongDistanceInfoAsync(connectionId);
    
    _cache.Set(cacheKey, info, TimeSpan.FromSeconds(30));
    
    return info;
}
```

## Step 6: Real-time Updates (Optional)

If you have SignalR implemented, add real-time location updates:

```csharp
// When user updates location
await _realTimeSyncService.BroadcastToPartnerAsync(
    userId, 
    "LocationUpdated", 
    new { Latitude = lat, Longitude = lon, TimeZoneId = tzId });

// In receiving client
_hubConnection.On<LocationUpdateDto>("LocationUpdated", async (update) =>
{
    await DistanceWidgetViewModel.RefreshAsync();
});
```

## Step 7: Notifications (Optional)

Add notifications for meeting reminders:

```csharp
// In a background service or scheduled task
public async Task CheckMeetingRemindersAsync()
{
    var connections = await _connectionRepository.GetAllActiveAsync();
    
    foreach (var connection in connections)
    {
        if (connection.NextMeetingDate.HasValue)
        {
            var timeUntil = connection.NextMeetingDate.Value - DateTime.UtcNow;
            
            // Notify 24 hours before
            if (timeUntil.TotalHours <= 24 && timeUntil.TotalHours > 23)
            {
                await SendMeetingReminderAsync(connection);
            }
        }
    }
}
```

## Step 8: Privacy and Permissions

### Location Privacy Notice

Add a privacy notice when users first access location settings:

```xml
<Border Background="#FFF3E0" 
       BorderBrush="#FF9800"
       BorderThickness="1"
       CornerRadius="4"
       Padding="12"
       Margin="0,0,0,16">
    <StackPanel>
        <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
            <materialDesign:PackIcon Kind="ShieldLock" 
                                   VerticalAlignment="Top"
                                   Foreground="#FF9800"
                                   Margin="0,0,8,0"/>
            <TextBlock Text="Privacy Notice" 
                      Style="{StaticResource MaterialDesignBody2TextBlock}"
                      FontWeight="SemiBold"/>
        </StackPanel>
        <TextBlock Text="Your location is only shared with your connected partner and is used to calculate distance and optimal communication times. You can clear your location at any time."
                  Style="{StaticResource MaterialDesignCaptionTextBlock}"
                  TextWrapping="Wrap"/>
    </StackPanel>
</Border>
```

### Permission Tracking

Track when users grant location permission:

```csharp
public class User
{
    public bool LocationSharingEnabled { get; private set; }
    public DateTime? LocationSharingGrantedAt { get; private set; }
    
    public void EnableLocationSharing()
    {
        LocationSharingEnabled = true;
        LocationSharingGrantedAt = DateTime.UtcNow;
    }
    
    public void DisableLocationSharing()
    {
        LocationSharingEnabled = false;
        Latitude = null;
        Longitude = null;
        TimeZoneId = null;
    }
}
```

## Step 9: Error Handling

Add global error handling for location services:

```csharp
try
{
    await _longDistanceService.UpdateUserLocationAsync(userId, lat, lon, tzId);
}
catch (ValidationException ex)
{
    // Show validation errors to user
    foreach (var error in ex.Errors)
    {
        ShowError($"{error.Key}: {string.Join(", ", error.Value)}");
    }
}
catch (NotFoundException ex)
{
    // User or connection not found
    ShowError("Unable to update location. Please try again.");
}
catch (Exception ex)
{
    // Log unexpected errors
    _logger.LogError(ex, "Error updating location");
    ShowError("An unexpected error occurred. Please try again later.");
}
```

## Step 10: Documentation

Update user-facing documentation:

1. Add help section explaining how to find coordinates
2. Create FAQ for common timezone issues
3. Document privacy policy for location data
4. Add troubleshooting guide

## Verification Checklist

Before deploying to production:

- [ ] All database migrations applied successfully
- [ ] Services registered in DI container
- [ ] Navigation integrated
- [ ] Manual testing completed
- [ ] Edge cases handled
- [ ] Error handling implemented
- [ ] Privacy notices added
- [ ] Performance optimized
- [ ] Documentation updated
- [ ] Code reviewed
- [ ] Build succeeds without warnings

## Troubleshooting

### Distance Not Showing
- Verify both users have set their location
- Check database for latitude/longitude values
- Verify Haversine formula calculation

### Countdown Timer Not Updating
- Check DispatcherTimer is started
- Verify timer is not disposed prematurely
- Check for exceptions in LoadDataAsync

### Timezone Issues
- Verify TimeZoneInfo.FindSystemTimeZoneById works
- Check timezone IDs are valid
- Test with various timezone combinations

### Communication Window Not Showing
- Verify both users have timezones set
- Check if there's actually an overlap
- Test with timezones that should overlap

## Support

For issues or questions:
1. Check the README_LongDistance.md for detailed documentation
2. Review the implementation in LongDistanceService.cs
3. Test with the manual testing checklist
4. Check application logs for errors

## Conclusion

Following this integration guide will ensure the long-distance features are properly integrated into the Together application with proper error handling, privacy considerations, and performance optimization.
