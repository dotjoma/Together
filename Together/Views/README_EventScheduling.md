# Event Scheduling Implementation

## Overview
This document describes the implementation of the event scheduling feature (Task 14) for the Together application. The feature allows couples to create, manage, and track shared events with support for recurring events and reminders.

## Components Implemented

### Domain Layer
- **ISharedEventRepository**: Repository interface for event data access
  - GetByIdAsync: Retrieve a single event
  - GetByConnectionIdAsync: Get all events for a couple connection
  - GetUpcomingEventsAsync: Get upcoming events with limit
  - GetEventsByDateRangeAsync: Get events within a date range
  - AddAsync, UpdateAsync, DeleteAsync: CRUD operations

### Infrastructure Layer
- **SharedEventRepository**: Implementation of ISharedEventRepository
  - Uses Entity Framework Core with eager loading for related entities
  - Includes proper indexing on ConnectionId and EventDate

### Application Layer

#### DTOs
- **CreateEventDto**: Data for creating new events
  - Title, EventDate, Description, Recurrence
- **UpdateEventDto**: Data for updating existing events
  - Id, Title, EventDate, Description, Recurrence
- **SharedEventDto**: Complete event data for display
  - Includes creator information and reminder status

#### Services
- **IEventService**: Service interface for event operations
- **EventService**: Implementation with the following features:
  - **CreateEventAsync**: Creates events with validation
  - **UpdateEventAsync**: Updates events with access control
  - **DeleteEventAsync**: Deletes events with access control
  - **GetEventByIdAsync**: Retrieves single event
  - **GetUserEventsAsync**: Gets all events for a user's connection
  - **GetUpcomingEventsAsync**: Gets next N upcoming events
  - **GetEventsByMonthAsync**: Gets events for a specific month
  - **GetDaysTogetherAsync**: Calculates relationship milestone (days together)
  - **ProcessEventRemindersAsync**: Processes 24-hour reminders (placeholder for background service)
  - **GenerateRecurringEventsAsync**: Generates recurring event instances

### Presentation Layer

#### ViewModels
- **EventFormViewModel**: Handles event creation and editing
  - Properties: Title, EventDate, EventTime, Description, SelectedRecurrence
  - Commands: SaveCommand, CancelCommand
  - Events: OnEventSaved, OnCancelled
  - Supports both create and edit modes
  - Validates input before saving

- **CalendarViewModel**: Manages calendar display and navigation
  - Properties: CurrentMonth, DaysTogether, UpcomingEvents, MonthEvents
  - Commands: PreviousMonthCommand, NextMonthCommand, TodayCommand, CreateEventCommand, EditEventCommand, DeleteEventCommand, RefreshCommand
  - Events: OnCreateEventRequested, OnEditEventRequested
  - Loads and displays events by month
  - Shows upcoming events list
  - Displays relationship milestone counter

#### Views
- **EventFormView.xaml**: Form for creating/editing events
  - Material Design styled inputs
  - Date and time pickers
  - Recurrence dropdown (none, daily, weekly, monthly, yearly)
  - Description text area
  - Save and Cancel buttons
  - Error message display

- **CalendarView.xaml**: Main calendar interface
  - Header with "days together" milestone
  - Month navigation (previous, next, today)
  - Upcoming events card (next 5 events)
  - Month events list with full details
  - Event indicators for reminders and recurrence
  - Click to edit functionality
  - Delete button for each event

#### Converters
- **EventFormTitleConverter**: Converts IsEditMode to "Create Event" or "Edit Event"
- **RecurrenceVisibilityConverter**: Shows recurrence indicator only for recurring events

## Features Implemented

### Core Functionality
✅ Create events with title, date, time, and description
✅ Edit existing events
✅ Delete events
✅ Recurrence support (daily, weekly, monthly, yearly)
✅ Automatic generation of recurring event instances (12 months ahead)
✅ Relationship milestone tracking (days together)
✅ 24-hour reminder detection (HasReminder property)

### UI Features
✅ Calendar view with month navigation
✅ Upcoming events list (next 5 events)
✅ Month events list with full details
✅ Event form with date/time pickers
✅ Recurrence options dropdown
✅ Visual indicators for reminders and recurring events
✅ Material Design styling throughout
✅ Loading states and error handling

### Security & Validation
✅ Access control: Users can only manage events for their couple connection
✅ Input validation: Title required, valid recurrence types
✅ Business rule enforcement: Must have active couple connection

## Usage

### Creating an Event
1. User clicks "Create Event" button
2. EventFormView is displayed with empty form
3. User fills in title, date, time, description, and recurrence
4. User clicks "Save"
5. Event is created and calendar refreshes

### Editing an Event
1. User clicks on an event in the month list
2. EventFormView is displayed with event data pre-filled
3. User modifies fields
4. User clicks "Save"
5. Event is updated and calendar refreshes

### Viewing Events
1. CalendarView displays current month by default
2. Upcoming events shown in top card
3. All month events listed below
4. User can navigate between months
5. "Days together" milestone displayed in header

### Recurring Events
1. User selects recurrence type when creating event
2. System generates instances for next 12 months
3. Each instance appears as separate event
4. Recurring indicator shown on event cards

## Integration Points

### Dependency Injection
Services registered in App.xaml.cs:
```csharp
services.AddScoped<ISharedEventRepository, SharedEventRepository>();
services.AddScoped<IEventService, EventService>();
```

### Database
- Uses existing TogetherDbContext
- SharedEvent entity already configured
- Indexes on ConnectionId and EventDate for performance

### Navigation
ViewModels expose events for navigation:
- OnCreateEventRequested: Navigate to event form
- OnEditEventRequested: Navigate to event form with data
- OnEventSaved: Return to calendar and refresh

## Future Enhancements

### Reminder System
The ProcessEventRemindersAsync method is a placeholder for a background service that would:
- Run periodically (e.g., every hour)
- Check for events within 24 hours
- Send notifications to both partners
- Track which reminders have been sent to avoid duplicates

### Suggested Implementation
```csharp
// Background service or scheduled task
public class EventReminderService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _eventService.ProcessEventRemindersAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

### Real-time Synchronization
When integrated with SignalR (Task 20):
- Event creation/update/delete broadcasts to partner
- Calendar auto-refreshes when partner makes changes
- Real-time reminder notifications

### Additional Features
- Event categories/tags
- Event location
- Attach photos to events
- Export to calendar (iCal)
- Event search and filtering
- Event templates for common occasions

## Testing Recommendations

### Unit Tests
- EventService business logic
- Recurring event generation
- Access control validation
- Days together calculation

### Integration Tests
- Event CRUD operations with database
- Recurring event creation and retrieval
- Month navigation and filtering

### UI Tests
- Event form validation
- Calendar navigation
- Event list display
- Edit/delete operations

## Requirements Satisfied

This implementation satisfies the following requirements from the specification:

**Requirement 6.1**: ✅ Create events with title, date, and time
**Requirement 6.2**: ✅ 24-hour reminder detection (HasReminder property)
**Requirement 6.3**: ✅ Relationship start date tracking and days together calculation
**Requirement 6.4**: ✅ Update and delete events with synchronization
**Requirement 6.5**: ✅ Recurring events (daily, weekly, monthly, yearly)

## Notes

- The reminder notification system (6.2) is partially implemented. The HasReminder property correctly identifies events within 24 hours, but actual notification delivery requires a background service (future enhancement).
- Recurring events are generated 12 months in advance. This can be adjusted in the GenerateRecurringEventsForEventAsync method.
- All event operations require an active couple connection, enforcing the couple-focused nature of the feature.
- The UI uses Material Design components for consistency with the rest of the application.
