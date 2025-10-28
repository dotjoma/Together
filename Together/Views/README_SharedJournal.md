# Shared Journal Implementation

## Overview
The shared journal feature allows partners in a couple connection to write and share journal entries with each other. Entries can include text content and optional images, creating a private timeline of shared thoughts and experiences.

## Components

### Domain Layer
- **JournalEntry Entity**: Core entity with content, author, timestamps, and read status
- **IJournalEntryRepository**: Repository interface for data access

### Application Layer
- **JournalService**: Business logic for creating, retrieving, marking as read, and deleting entries
- **IJournalService**: Service interface
- **DTOs**:
  - `JournalEntryDto`: Full entry data transfer object
  - `CreateJournalEntryDto`: DTO for creating new entries

### Infrastructure Layer
- **JournalEntryRepository**: EF Core implementation of journal repository
- **JournalEntryConfiguration**: Entity configuration (already exists)

### Presentation Layer
- **JournalViewModel**: Main view model coordinating the journal view
- **JournalEntryViewModel**: View model for creating new entries
- **JournalEntryItemViewModel**: View model for displaying individual entries
- **JournalView.xaml**: XAML view with timeline layout

## Features Implemented

### 1. Create Journal Entry
- Text content input with multi-line support
- Image attachment with 5MB size limit
- Automatic timestamp and author tracking
- Validation to ensure user is part of the couple connection

### 2. Display Journal Entries
- Timeline format showing entries in reverse chronological order
- Author information with profile picture or initial
- Formatted timestamps (e.g., "Jan 15, 2024 at 3:45 PM")
- Image display with proper sizing
- Empty state when no entries exist

### 3. Read Status Tracking
- Automatic marking of entries as read when viewed
- Visual indicator (checkmark) for read entries
- Only partner (not author) can mark entries as read

### 4. Delete Entry
- Authors can delete their own entries
- Confirmation through delete button
- Automatic cleanup of associated images from storage
- Real-time removal from timeline

### 5. Image Support
- Image upload through file picker
- Preview before posting
- Remove image option
- Storage through SupabaseStorageService
- 5MB size limit validation

## UI/UX Features

### Entry Creation Card
- Prominent text input area at the top
- Character counter (if needed)
- Image attachment button
- Preview of selected image with remove option
- Post button with loading state
- Error message display

### Timeline Display
- Card-based layout for each entry
- Author avatar (profile picture or initial)
- Author name and timestamp
- Entry content with text wrapping
- Image display (if present)
- Read indicator for partner-viewed entries
- Delete button for own entries (visible only to author)

### Visual Indicators
- Read status: Green checkmark icon
- Loading states: Progress bars during operations
- Empty state: Book icon with helpful message
- Error states: Red error messages

## Business Rules

1. **Connection Validation**: Users must be part of the couple connection to create entries
2. **Author Permissions**: Only entry authors can delete their entries
3. **Read Status**: Only the partner (not the author) can mark entries as read
4. **Image Size**: Maximum 5MB per image
5. **Automatic Read Marking**: Entries are marked as read when the partner views the journal

## Data Flow

### Creating an Entry
1. User enters content and optionally selects an image
2. If image selected, it's uploaded to Supabase Storage
3. JournalService validates the connection and user membership
4. New JournalEntry entity is created with timestamp
5. Entry is saved to database
6. UI is updated with the new entry at the top of the timeline

### Viewing Entries
1. JournalViewModel loads all entries for the connection
2. Entries are displayed in reverse chronological order
3. Unread entries (where author ≠ current user) are automatically marked as read
4. Read status is updated in the database

### Deleting an Entry
1. User clicks delete button on their own entry
2. Service validates the user is the author
3. Associated image is deleted from storage (if exists)
4. Entry is removed from database
5. UI removes the entry from the timeline

## Integration Points

### Services Used
- **IJournalService**: Core journal operations
- **IStorageService**: Image upload and deletion
- **ICoupleConnectionRepository**: Connection validation

### Dependencies
- Material Design for UI components
- Entity Framework Core for data access
- Supabase for storage

## Usage Example

```csharp
// In a view or window that needs to display the journal
var journalService = serviceProvider.GetRequiredService<IJournalService>();
var currentUserId = GetCurrentUserId();
var connectionId = GetUserConnectionId();

var viewModel = new JournalViewModel(journalService, currentUserId, connectionId);
var view = new JournalView { DataContext = viewModel };
```

## Future Enhancements

1. **Rich Text Editing**: Support for formatting (bold, italic, lists)
2. **Multiple Images**: Allow multiple images per entry
3. **Entry Editing**: Allow authors to edit their entries
4. **Search and Filter**: Search entries by content or date
5. **Export**: Export journal as PDF or text file
6. **Reactions**: Allow partners to react to entries with emojis
7. **Real-time Updates**: Use SignalR to show new entries immediately
8. **Notifications**: Notify partner when new entry is created

## Requirements Satisfied

- ✅ 3.1: Create journal entry with timestamp and author tracking
- ✅ 3.2: Display entries in chronological order
- ✅ 3.3: Mark entries as read and update read status
- ✅ 3.4: Image attachment support (5MB limit)
- ✅ 3.5: Delete journal entries
