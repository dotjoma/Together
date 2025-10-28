# Post Creation and Management Implementation

## Overview
This document describes the implementation of Task 7: Post creation and management functionality for the Together application.

## Components Implemented

### 1. Backend Services (Task 7.1)

#### PostRepository
- **Location**: `Together.Infrastructure/Repositories/PostRepository.cs`
- **Functionality**:
  - `GetByIdAsync`: Retrieves a post with all related data (author, images, likes, comments)
  - `GetUserPostsAsync`: Gets paginated posts for a specific user
  - `GetFeedPostsAsync`: Gets paginated posts from users that the current user follows
  - `AddAsync`: Creates a new post
  - `UpdateAsync`: Updates an existing post
  - `DeleteAsync`: Deletes a post and all associated data

#### PostService
- **Location**: `Together.Application/Services/PostService.cs`
- **Functionality**:
  - `CreatePostAsync`: Creates a new post with validation
    - Validates content length (max 500 characters)
    - Validates image count (max 4 images)
    - Validates image size (max 5MB each)
    - Uploads images to storage
  - `UpdatePostAsync`: Updates post content
    - Validates user is the author
    - Enforces 15-minute edit window
    - Validates content length
  - `DeletePostAsync`: Deletes a post
    - Validates user is the author
    - Removes images from storage
    - Cascades deletion to likes and comments
  - `GetPostByIdAsync`: Retrieves a single post
  - `GetUserPostsAsync`: Gets paginated user posts
  - `GetFeedPostsAsync`: Gets paginated feed posts

#### DTOs
- **CreatePostDto**: Contains content and optional image paths
- **UpdatePostDto**: Contains post ID and new content
- **PostDto**: Complete post data including author, content, timestamps, counts, and image URLs

#### Storage Service Extensions
- **Location**: `Together.Infrastructure/Services/SupabaseStorageService.cs`
- Added methods:
  - `UploadImageAsync`: Uploads an image from file path to storage
  - `DeleteImageAsync`: Deletes an image from storage

### 2. Post Creation UI (Task 7.2)

#### PostCreationViewModel
- **Location**: `Together/ViewModels/PostCreationViewModel.cs`
- **Features**:
  - Content input with character counter (500 max)
  - Image attachment support (up to 4 images)
  - Image size validation (5MB max per image)
  - Edit mode for existing posts (within 15 minutes)
  - Real-time validation
  - Loading states during post creation
  - Error message display
  - Events: `PostCreated`, `PostUpdated`, `Cancelled`

#### PostCreationView
- **Location**: `Together/Views/PostCreationView.xaml`
- **UI Elements**:
  - Multi-line text input with Material Design styling
  - Character counter showing remaining characters
  - Image preview grid with remove buttons
  - Add Images button (hidden in edit mode)
  - Cancel and Post/Update buttons
  - Loading indicator during posting
  - Error message display

### 3. Post Display Components (Task 7.3)

#### PostCardViewModel
- **Location**: `Together/ViewModels/PostCardViewModel.cs`
- **Features**:
  - Displays post data
  - Calculates "time ago" display (e.g., "5m ago", "2h ago")
  - Shows edit indicator for edited posts
  - Determines if user can edit (own post + within 15 minutes)
  - Edit command with validation
  - Delete command with confirmation
  - Events: `EditRequested`, `PostDeleted`

#### PostCard Control
- **Location**: `Together/Controls/PostCard.xaml`
- **UI Elements**:
  - Author profile picture (circular)
  - Author username
  - Timestamp with "time ago" format
  - Edit indicator for edited posts
  - Post content with text wrapping
  - Image grid (up to 4 images in 2x2 layout)
  - Like count with heart icon
  - Comment count with comment icon
  - Menu button for own posts (edit/delete)
  - Loading overlay during deletion

### 4. Supporting Components

#### BooleanToTextConverter
- **Location**: `Together/Converters/BooleanToTextConverter.cs`
- **Purpose**: Converts boolean values to conditional text (e.g., "Edit Post" vs "Create Post")
- **Usage**: `{Binding IsEditing, Converter={StaticResource BoolToText}, ConverterParameter='Edit Post|Create Post'}`

## Requirements Satisfied

### Requirement 13.1: Post Creation
✅ Users can create posts with text content up to 500 characters
✅ Posts are timestamped and displayed chronologically
✅ Content validation enforced

### Requirement 13.2: Image Attachments
✅ Users can attach up to 4 images per post
✅ Maximum 5MB per image enforced
✅ Image previews shown before posting
✅ Images uploaded to storage service

### Requirement 13.3: Post Display
✅ Posts display author information
✅ Posts show content, timestamp, and images
✅ Like and comment counts displayed
✅ Time ago format for timestamps

### Requirement 13.4: Post Editing
✅ Users can edit their own posts
✅ 15-minute edit window enforced
✅ Edit indicator shown on edited posts
✅ Content validation on edit

### Requirement 13.5: Post Deletion
✅ Users can delete their own posts
✅ Associated images removed from storage
✅ Cascading deletion of likes and comments
✅ Authorization check enforced

## Integration Points

### Required Services
The following services must be registered in the DI container:
```csharp
services.AddScoped<IPostRepository, PostRepository>();
services.AddScoped<IPostService, PostService>();
```

### Dependencies
- `IUserRepository`: For author information
- `IStorageService`: For image upload/deletion
- `TogetherDbContext`: For database access

### Navigation
To use the post creation UI:
```csharp
var viewModel = new PostCreationViewModel(postService, currentUserId);
var view = new PostCreationView { DataContext = viewModel };

// Subscribe to events
viewModel.PostCreated += (s, e) => { /* Handle post created */ };
viewModel.PostUpdated += (s, e) => { /* Handle post updated */ };
```

To display a post:
```csharp
var viewModel = new PostCardViewModel(postService, currentUserId, postDto);
var card = new PostCard { DataContext = viewModel };

// Subscribe to events
viewModel.EditRequested += (s, post) => { /* Open edit UI */ };
viewModel.PostDeleted += (s, postId) => { /* Remove from UI */ };
```

## Testing Considerations

### Unit Tests
- PostService validation logic
- Edit window enforcement
- Image size validation
- Authorization checks

### Integration Tests
- Post creation with images
- Post editing within/outside time window
- Post deletion with cascading
- Feed retrieval with pagination

### UI Tests
- Character counter updates
- Image preview display
- Edit mode toggle
- Error message display

## Future Enhancements
- Draft saving
- Post scheduling
- Rich text formatting
- Video attachments
- Post sharing
- Post reporting
