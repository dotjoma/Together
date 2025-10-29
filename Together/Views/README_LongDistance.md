# Long Distance Features Implementation

## Overview
This document describes the implementation of long-distance relationship features for the Together application, including distance calculation, timezone management, countdown timers, and optimal communication windows.

## Components Implemented

### 1. Domain Layer

#### Entity Updates
- **User Entity** (`Together.Domain/Entities/User.cs`)
  - Added `Latitude`, `Longitude`, and `TimeZoneId` properties
  - Added `UpdateLocation()` method for updating location data

- **CoupleConnection Entity** (`Together.Domain/Entities/CoupleConnection.cs`)
  - Added `NextMeetingDate` property
  - Added `SetNextMeetingDate()` method

#### Database Configuration
- **UserConfiguration** - Added columns for latitude, longitude, and timezone_id
- **CoupleConnectionConfiguration** - Added column for next_meeting_date

### 2. Application Layer

#### DTOs
- **LocationDto** - Represents location data (latitude, longitude, timezone)
- **LongDistanceInfoDto** - Comprehensive DTO containing:
  - Distance in kilometers and miles
  - Both users' timezones and local times
  - Time difference between partners
  - Optimal communication window
  - Next meeting date and countdown

- **CommunicationWindowDto** - Represents optimal communication times for both partners

#### Service Interface
- **ILongDistanceService** (`Together.Application/Interfaces/ILongDistanceService.cs`)
  - `GetLongDistanceInfoAsync()` - Retrieves all long-distance information
  - `UpdateUserLocationAsync()` - Updates user location and timezone
  - `SetNextMeetingDateAsync()` - Sets the next meeting date
  - `CalculateDistance()` - Calculates distance using Haversine formula
  - `CalculateOptimalCommunicationWindow()` - Finds overlapping communication windows

#### Service Implementation
- **LongDistanceService** (`Together.Application/Services/LongDistanceService.cs`)
  - Implements Haversine formula for accurate distance calculation
  - Handles timezone conversions using .NET TimeZoneInfo
  - Calculates optimal communication windows (8 AM - 10 PM in each timezone)
  - Validates location coordinates and timezone identifiers
  - Computes countdown to next meeting date

### 3. Presentation Layer

#### Controls
- **DistanceWidget** (`Together/Controls/DistanceWidget.xaml`)
  - Displays distance between partners in km/miles
  - Shows countdown timer (days, hours, minutes, seconds)
  - Displays both partners' local times with timezone labels
  - Shows optimal communication window
  - Provides button to set next meeting date

#### Views
- **LongDistanceView** (`Together/Views/LongDistanceView.xaml`)
  - Main view combining all long-distance features
  - Includes DistanceWidget
  - Provides tips for long-distance relationships
  - Button to access location settings

- **LocationSettingsView** (`Together/Views/LocationSettingsView.xaml`)
  - Form for entering latitude and longitude
  - Timezone selector with all system timezones
  - Help text explaining how to find coordinates
  - Save and clear location buttons

- **SetNextMeetingView** (`Together/Views/SetNextMeetingView.xaml`)
  - Date picker for selecting meeting date
  - Time picker for selecting meeting time
  - Validation for future dates only

#### ViewModels
- **DistanceWidgetViewModel** (`Together/ViewModels/DistanceWidgetViewModel.cs`)
  - Loads and displays long-distance information
  - Updates every second for real-time countdown
  - Formats distance, time, and timezone displays
  - Handles setting next meeting date

- **LocationSettingsViewModel** (`Together/ViewModels/LocationSettingsViewModel.cs`)
  - Manages location input and validation
  - Provides list of all available timezones
  - Validates latitude (-90 to 90) and longitude (-180 to 180)
  - Saves location to user profile

- **SetNextMeetingViewModel** (`Together/ViewModels/SetNextMeetingViewModel.cs`)
  - Manages next meeting date selection
  - Combines date and time into single DateTime
  - Validates future dates only
  - Converts to UTC for storage

- **LongDistanceViewModel** (`Together/ViewModels/LongDistanceViewModel.cs`)
  - Coordinates all long-distance features
  - Manages navigation to location settings

## Key Features

### 1. Distance Calculation
- Uses Haversine formula for accurate great-circle distance
- Supports both kilometers and miles
- Requires both users to share location

### 2. Timezone Management
- Displays current local time for both partners
- Shows timezone labels for clarity
- Calculates time difference between partners
- Supports all system timezones

### 3. Optimal Communication Windows
- Defines reasonable hours as 8 AM - 10 PM
- Calculates overlapping windows in both timezones
- Displays overlap in user's local time
- Returns null if no overlap exists

### 4. Countdown Timer
- Real-time countdown to next meeting
- Displays days, hours, minutes, and seconds
- Updates every second using DispatcherTimer
- Shows meeting date in readable format

### 5. Location Privacy
- Location sharing is optional
- Users must explicitly provide coordinates
- Location can be cleared at any time
- Features gracefully degrade without location data

## Usage

### Setting Up Location
1. Navigate to Long Distance view
2. Click "Update Location Settings"
3. Enter latitude and longitude (from Google Maps)
4. Select your timezone
5. Click "Save"

### Setting Next Meeting Date
1. In the DistanceWidget, click "Set Next Meeting Date"
2. Select date and time
3. Click "Set Date"
4. Countdown timer will appear

### Viewing Long Distance Info
- Distance is displayed automatically when both users have location
- Local times update in real-time
- Optimal communication window shows best times to connect
- Countdown timer updates every second

## Technical Details

### Haversine Formula
```csharp
var dLat = DegreesToRadians(lat2 - lat1);
var dLon = DegreesToRadians(lon2 - lon1);

var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
return EarthRadiusKm * c;
```

### Communication Window Algorithm
1. Define 8 AM - 10 PM window in each timezone
2. Convert both windows to UTC
3. Find overlap: max(start1, start2) to min(end1, end2)
4. Convert overlap back to local times for display

### Real-time Updates
- DispatcherTimer updates every 1 second
- Reloads all data to ensure accuracy
- Updates countdown, local times, and optimal windows

## Requirements Satisfied

### Requirement 10.1
✅ Distance calculation using Haversine formula
✅ Display distance in km/miles

### Requirement 10.2
✅ Countdown timer for next meeting date
✅ Days, hours, minutes, seconds display

### Requirement 10.3
✅ Display both partners' local times
✅ Timezone labels shown
✅ Optimal communication windows (8 AM - 10 PM)

### Requirement 10.4
✅ Location permission handling (explicit user input)
✅ Optional location sharing
✅ Graceful degradation without location

## Future Enhancements
- Automatic location detection using IP geolocation
- Map visualization showing both locations
- Historical distance tracking
- Meeting countdown notifications
- Timezone converter tool
- Travel time estimates
- Weather comparison between locations

## Dependencies
- Microsoft.EntityFrameworkCore (database)
- MaterialDesignThemes (UI components)
- System.Device.Location (future: automatic location)

## Testing Recommendations
1. Test with various timezone combinations
2. Verify Haversine formula accuracy
3. Test countdown timer accuracy
4. Validate location input ranges
5. Test with missing location data
6. Verify optimal window calculations
7. Test date/time conversions across timezones
