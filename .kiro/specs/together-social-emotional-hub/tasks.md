# Implementation Plan

## Project Setup and Foundation

- [x] 1. Set up Clean Architecture project structure

  - Create four class library projects: Together.Domain, Together.Application, Together.Infrastructure, Together.Presentation (WPF)
  - Configure project references following dependency rules (Presentation → Application → Domain ← Infrastructure)
  - Add NuGet packages: MaterialDesignThemes, EF Core, Npgsql, BCrypt.Net, Supabase client libraries
  - Set up dependency injection container in App.xaml.cs
  - _Requirements: 1.1, 1.2, 18.1, 18.2_

- [x] 2. Implement domain entities and value objects

  - Create User, CoupleConnection, Post, MoodEntry, JournalEntry, TodoItem, SharedEvent, Challenge, VirtualPet entities
  - Implement Email value object with validation
  - Define enums: MoodType, ProfileVisibility, ConnectionStatus, InteractionType, PetState
  - Add domain interfaces: IUserRepository, ICoupleConnectionRepository, IPostRepository, IMoodEntryRepository
  - _Requirements: 1.1, 2.3, 3.1, 4.1, 11.1_

- [x] 3. Configure database context and migrations

  - Create TogetherDbContext with DbSet properties for all entities
  - Implement entity configurations using Fluent API for constraints and relationships
  - Configure Supabase connection string in appsettings.json
  - Create initial database migration with all tables
  - _Requirements: 1.1, 18.3, 18.4_

## Authentication and User Management

- [x] 4. Implement authentication service and user registration

- [x] 4.1 Create AuthenticationService with BCrypt password hashing

  - Implement RegisterAsync method with password validation (8+ chars, uppercase, lowercase, number)

  - Implement LoginAsync method with JWT token generation
  - Create password reset request and reset methods
  - _Requirements: 1.1, 1.2, 1.4, 1.5, 18.1_

- [x] 4.2 Build registration and login UI

  - Create LoginView and RegisterView with Material Design styling
  - Implement LoginViewModel and RegisterViewModel with validation

  - Add RelayCommand for login and register actions
  - Display validation errors inline with form fields
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 4.3 Implement user repository

  - Create UserRepository implementing IUserRepository
  - Implement GetByIdAsync, GetByEmailAsync, GetByUsernameAsync, AddAsync, UpdateAsync methods
  - Add user search functionality with privacy filtering

  - _Requirements: 1.1, 11.5_

- [x] 4.4 Write authentication tests

  - Create unit tests for password validation logic
  - Test JWT token generation and validation
  - Test login with invalid credentials
  - _Requirements: 1.3, 1.4_

## User Profile and Social Features

- [x] 5. Implement user profile management

- [x] 5.1 Create profile service and repository

  - Implement profile update methods (bio, profile picture, visibility settings)
  - Create SupabaseStorageService for profile picture uploads with 2MB limit

  - Implement image compression and URL generation
  - _Requirements: 11.2, 11.3, 11.4_

- [x] 5.2 Build user profile UI

  - Create UserProfileView displaying username, bio, profile picture, follower/following counts
  - Implement UserProfileViewModel with edit mode
  - Add image picker for profile picture upload
  - Implement visibility toggle (public, friends-only, private)
  - _Requirements: 11.2, 11.3, 11.4_

- [x] 6. Implement follow system

- [x] 6.1 Create follow relationship service

  - Implement SendFollowRequestAsync creating pending follow relationships
  - Create AcceptFollowRequestAsync and RejectFollowRequestAsync methods
  - Implement UnfollowAsync removing follow relationships

  - Add GetFollowersAsync and GetFollowingAsync methods
  - _Requirements: 12.1, 12.2, 12.3, 12.5_

- [x] 6.2 Build follow UI components

  - Create FollowButton control with pending/following states
  - Implement FollowRequestsView showing pending requests
  - Create FollowerListView and FollowingListView
  - Display follower and following counts on user walls
  - _Requirements: 12.2, 12.4_

- [x] 7. Implement post creation and management

- [x] 7.1 Create post service and repository

  - Implement CreatePostAsync with 500 character limit validation
  - Add image attachment support (up to 4 images, 5MB each)
  - Implement EditPostAsync with 15-minute time window check
  - Create DeletePostAsync removing post and associated data
  - _Requirements: 13.1, 13.2, 13.4, 13.5_

- [x] 7.2 Build post creation UI

  - Create PostCreationView with text input and character counter
  - Implement PostCreationViewModel with validation
  - Add image picker supporting multiple image selection
  - Display image previews before posting
  - Show edit option for posts within 15 minutes
  - _Requirements: 13.1, 13.2, 13.4_

- [x] 7.3 Implement post display components

  - Create PostCard control displaying author, content, timestamp, images
  - Add like and comment count displays
  - Implement post menu with edit/delete options for own posts
  - _Requirements: 13.1, 13.3_

- [x] 8. Implement social feed

- [x] 8.1 Create social feed service

  - Implement GetFeedAsync aggregating posts from followed users
  - Add pagination with 20 posts per page

  - Create GetSuggestedUsersAsync based on mutual connections
  - Implement feed caching with cache invalidation
  - _Requirements: 14.1, 14.2, 14.5_

- [x] 8.2 Build social feed UI

  - Create SocialFeedView with virtualized scrolling
  - Implement SocialFeedViewModel with infinite scroll
  - Display suggested users when no follow relationships exist
  - Add pull-to-refresh functionality
  - _Requirements: 14.1, 14.2, 14.4, 14.5_

- [x] 9. Implement post interactions (likes and comments)

- [x] 9.1 Create like and comment services

  - Implement ToggleLikeAsync incrementing/decrementing like count
  - Create AddCommentAsync with 300 character limit
  - Implement GetCommentsAsync with pagination
  - Add notification creation for likes and comments
  - _Requirements: 15.1, 15.2, 15.3, 15.5_

- [x] 9.2 Build interaction UI components

  - Create LikeButton with filled/unfilled states
  - Implement CommentSection displaying comments chronologically
  - Add CommentInput with character counter
  - Display author profile pictures in comments
  - _Requirements: 15.1, 15.3, 15.4_

## Couple Connection Features

- [x] 10. Implement couple connection system

- [x] 10.1 Create couple connection service

  - Implement SendConnectionRequestAsync creating pending requests
  - Add AcceptConnectionRequestAsync establishing mutual connection
  - Enforce one-connection-per-user rule in business logic
  - Create TerminateConnectionAsync archiving shared data
  - _Requirements: 2.1, 2.3, 2.4, 2.5_

- [x] 10.2 Build connection request UI

  - Create ConnectionRequestView for sending requests
  - Implement ConnectionRequestNotificationView for receiving requests
  - Add accept/reject buttons with confirmation dialogs
  - Display current connection status
  - _Requirements: 2.1, 2.2, 2.3_

- [x] 11. Implement shared journal

- [x] 11.1 Create journal service and repository

  - Implement CreateJournalEntryAsync with timestamp and author tracking
  - Add image attachment support (5MB limit)
  - Create GetJournalEntriesAsync returning chronological entries
  - Implement MarkAsReadAsync updating read status
  - Create DeleteJournalEntryAsync
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 11.2 Build shared journal UI

  - Create JournalView displaying entries in timeline format
  - Implement JournalEntryViewModel with create/edit modes
  - Add rich text editor for journal content
  - Display read/unread indicators
  - Show author information and timestamps
  - _Requirements: 3.1, 3.2, 3.3_

- [x] 12. Implement mood tracking

- [x] 12.1 Create mood tracking service

  - Implement CreateMoodEntryAsync with mood type and notes
  - Create GetMoodHistoryAsync returning 30 days of data
  - Implement MoodAnalysisService calculating trends and patterns
  - Add GenerateSupportMessageAsync for negative moods
  - Create partner notification for negative moods
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [x] 12.2 Build mood tracking UI

  - Create MoodSelectorView with emoji-based mood selection
  - Implement MoodHistoryView with chart visualization (30 days)
  - Add notes input for mood entries
  - Display mood trends and patterns
  - Show supportive message suggestions
  - _Requirements: 4.1, 4.3, 4.4, 4.5_

- [x] 13. Implement shared to-do list

- [x] 13.1 Create todo service and repository

  - Implement CreateTodoItemAsync with title, due date, assignment
  - Add MarkAsCompleteAsync updating status and notifying partner
  - Create UpdateTodoItemAsync for editing
  - Implement overdue detection and highlighting logic
  - Add tag support for categorization
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 13.2 Build todo list UI

  - Create TodoListView displaying items with completion checkboxes
  - Implement TodoItemViewModel with edit functionality
  - Add assignment dropdown (user1, user2, both)
  - Display overdue items with visual indicators
  - Implement tag filtering
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 14. Implement event scheduling

- [x] 14.1 Create event service and repository

  - Implement CreateEventAsync with title, date, time, recurrence
  - Add reminder notification system (24 hours before)
  - Create UpdateEventAsync and DeleteEventAsync with sync
  - Implement relationship milestone tracking (days together)
  - Add recurring event generation logic
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 14.2 Build event calendar UI

  - Create CalendarView displaying events
  - Implement EventFormViewModel for create/edit
  - Add recurrence options (daily, weekly, monthly, yearly)
  - Display relationship milestone counter
  - Show upcoming events list
  - _Requirements: 6.1, 6.3, 6.5_

## Engagement Features

- [x] 15. Implement love streak tracking

- [x] 15.1 Create love streak service

  - Implement RecordInteractionAsync tracking daily interactions

  - Add streak increment logic for same-day interactions
  - Create CheckAndResetStreakAsync for 24-hour inactivity
  - Implement milestone detection (7, 30, 100, 365 days)
  - Add celebration notification generation
  - _Requirements: 7.1, 7.2, 7.4, 7.5_

- [x] 15.2 Build love streak display

  - Create StreakWidget showing current streak prominently
  - Implement milestone celebration animations
  - Display streak history graph
  - Add interaction type indicators
  - _Requirements: 7.3, 7.4_

- [x] 16. Implement challenge system

- [x] 16.1 Create challenge generator service

  - Implement challenge factory with categories (communication, fun, appreciation, learning)
  - Add GenerateDailyChallengeAsync running at midnight
  - Create CompleteChallengeAsync tracking completion by both partners
  - Implement points system and couple score tracking
  - Add ArchiveExpiredChallengesAsync for 24-hour expiration
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [x] 16.2 Build challenge UI

  - Create ChallengeCardView displaying challenge details
  - Implement ChallengeViewModel with completion tracking
  - Display points value and category
  - Show completion status for both partners
  - Add couple total score display
  - _Requirements: 8.2, 8.3_

- [x] 17. Implement virtual pet system

- [x] 17.1 Create virtual pet service

  - Implement pet creation on couple connection establishment
  - Add AddExperienceAsync calculating XP from interactions
  - Create level-up logic with appearance unlocks
  - Implement UpdatePetStateAsync detecting 3-day inactivity
  - Add CustomizePetAsync for name and appearance changes
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

- [x] 17.2 Build virtual pet UI

  - Create VirtualPetView with animated pet display
  - Implement PetCustomizationView for name and appearance
  - Display level, XP bar, and current state
  - Add pet state animations (happy, sad, neglected, excited)
  - Show unlocked appearance options
  - _Requirements: 9.1, 9.3, 9.4, 9.5_

## Long-Distance Support

- [x] 18. Implement long-distance features

- [x] 18.1 Create location and timezone service

  - Implement distance calculation using Haversine formula
  - Add timezone detection and display
  - Create optimal communication window calculator (8 AM - 10 PM)
  - Implement location permission handling
  - _Requirements: 10.1, 10.3, 10.4_

- [x] 18.2 Build long-distance UI

  - Create DistanceWidget displaying distance in km/miles
  - Implement CountdownTimer for next meeting date
  - Display both partners' local times with timezone labels
  - Show optimal communication windows
  - Add next meeting date setter
  - _Requirements: 10.1, 10.2, 10.3, 10.4_

## Couple Hub Dashboard

- [x] 19. Implement couple hub dashboard

- [x] 19.1 Create dashboard aggregation service

  - Implement GetDashboardSummaryAsync aggregating partner mood, streak, events
  - Add GetTogetherMomentsAsync returning last 5 activities
  - Create daily suggestion generator (positive activities, conversation starters)
  - Implement supportive message detection for negative moods
  - _Requirements: 17.1, 17.3, 17.4, 17.5_

- [x] 19.2 Build couple hub UI

  - Create CoupleHubView as main dashboard
  - Implement CoupleHubViewModel coordinating all widgets
  - Display partner mood, love streak, virtual pet, upcoming events
  - Show together moments feed
  - Add daily suggestion card
  - Display supportive message when partner is sad
  - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5_

## Real-time Synchronization

- [x] 20. Implement SignalR real-time service

- [x] 20.1 Create SignalR hub and client

  - Implement TogetherHub with connection management
  - Add group management for couples and followers
  - Create BroadcastToPartnerAsync and BroadcastToFollowersAsync methods
  - Implement reconnection logic with exponential backoff
  - Add connection status tracking
  - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5_

- [x] 20.2 Integrate real-time updates in ViewModels

  - Subscribe to SignalR events in relevant ViewModels
  - Implement real-time journal entry updates
  - Add real-time post feed updates
  - Create real-time mood synchronization
  - Display connection status indicator
  - _Requirements: 16.1, 16.2, 16.3, 16.5_

## Offline Support

- [x] 21. Implement offline sync manager

- [x] 21.1 Create offline sync service

  - Implement SQLite local cache for posts, journal entries, mood data
  - Add QueueOperationAsync for offline actions
  - Create SyncPendingOperationsAsync running on reconnection
  - Implement cache invalidation strategy
  - Add IsOnlineAsync network connectivity check
  - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5_

- [x] 21.2 Integrate offline support in UI

  - Display offline indicator when network unavailable
  - Queue journal entries, mood logs, todos locally
  - Prevent follow requests and post creation when offline
  - Show sync status during synchronization
  - _Requirements: 19.4, 19.5_

## Navigation and Main Window

- [x] 22. Implement navigation system

- [x] 22.1 Create navigation service

  - Implement NavigationService with ViewModel-based navigation
  - Add navigation history with back button support
  - Create module registration system
  - Implement parameter passing between views
  - _Requirements: 20.1_

- [x] 22.2 Build main window and navigation

  - Create MainWindow with navigation drawer
  - Implement MainViewModel coordinating navigation
  - Add navigation menu items (Couple Hub, Journal, Mood, Social Feed, Profile)
  - Display current user information in header
  - Implement logout functionality
  - _Requirements: 20.1_

## Error Handling and Logging

- [x] 23. Implement error handling infrastructure

- [x] 23.1 Create exception hierarchy and Result pattern

  - Implement TogetherException, AuthenticationException, ValidationException, NotFoundException
  - Create Result<T> pattern for service operations
  - Add global exception handler in App.xaml.cs
  - Implement user-friendly error message mapping
  - _Requirements: 1.3_

- [x] 23.2 Set up logging with Serilog

  - Configure Serilog with file and console sinks
  - Add structured logging throughout services
  - Implement correlation ID tracking
  - Ensure sensitive data exclusion from logs
  - _Requirements: 18.1_

## Performance Optimization

- [x] 24. Implement performance optimizations

- [x] 24.1 Add caching and virtualization

  - Implement in-memory caching for user session data
  - Add VirtualizingStackPanel to all list views
  - Create image lazy loading and caching
  - Implement debouncing for search inputs
  - _Requirements: 20.1, 20.4_

- [x] 24.2 Optimize database queries

  - Add indexes on foreign keys and common query fields
  - Implement pagination for all list queries
  - Configure lazy loading for navigation properties
  - Add connection pooling configuration
  - _Requirements: 20.1, 20.3_

## Security Implementation

- [x] 25. Implement security measures

- [x] 25.1 Add data protection and validation

  - Implement input validation and sanitization across all forms
  - Add SQL injection prevention via parameterized queries
  - Create secure token storage using Windows Credential Manager
  - Implement TLS 1.2+ enforcement for Supabase connections
  - _Requirements: 18.1, 18.2_

- [x] 25.2 Implement privacy controls

  - Add couple data isolation checks in all queries
  - Enforce profile visibility at repository level
  - Implement audit logging for sensitive operations
  - Add location permission handling
  - _Requirements: 18.3, 18.4_

## Final Integration and Polish

- [x] 26. Integrate all modules and test end-to-end flows

  - Wire up all services in dependency injection container
  - Test complete user journey from registration to couple features
  - Verify real-time synchronization across all features
  - Test offline mode and sync recovery
  - Validate all navigation paths
  - _Requirements: All_

- [x] 27. Apply Material Design theming and polish UI


  - Configure Material Design color scheme and typography
  - Add loading indicators for async operations
  - Implement smooth transitions and animations
  - Add empty state views for all lists
  - Ensure responsive layout for different window sizes
  - _Requirements: 20.1, 20.2_

- [ ] 28. Create deployment package
  - Configure ClickOnce deployment
  - Set up automatic update mechanism
  - Create installer with self-contained .NET runtime
  - Add application icon and branding
  - _Requirements: 20.3_
