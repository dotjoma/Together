# Design Document

## Overview

Together is a WPF desktop application built on .NET 8 that combines couple-focused features with social networking capabilities. The architecture follows Clean Architecture principles with clear separation between presentation, application, domain, and infrastructure layers. The system uses MVVM pattern for UI, Supabase for cloud data persistence, and SignalR for real-time communication.

### Technology Stack

- **Framework**: .NET 8 with WPF
- **UI Framework**: Material Design in XAML Toolkit
- **Architecture**: Clean Architecture + MVVM
- **Database**: Supabase (PostgreSQL-based cloud database)
- **Real-time**: SignalR for bidirectional communication
- **ORM**: Entity Framework Core with Npgsql provider
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Authentication**: Supabase Auth with JWT tokens
- **Image Storage**: Supabase Storage
- **Local Cache**: SQLite for offline data

### Design Principles Applied

- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **DRY**: Shared base classes and utilities to avoid code duplication
- **KISS**: Simple, straightforward implementations without over-engineering
- **Separation of Concerns**: Clear boundaries between layers
- **Dependency Injection**: Constructor injection throughout for testability and flexibility

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation Layer                      │
│  (WPF Views, ViewModels, Converters, Behaviors)        │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                       │
│  (Services, Commands, Queries, DTOs, Interfaces)       │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                          │
│  (Entities, Value Objects, Domain Events, Rules)       │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                     │
│  (Repositories, External Services, Database Context)    │
└─────────────────────────────────────────────────────────┘
```

### Project Structure

```
Together/
├── Together.Domain/              # Domain entities and interfaces
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   └── Interfaces/
├── Together.Application/         # Business logic and services
│   ├── Services/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Commands/
├── Together.Infrastructure/      # Data access and external services
│   ├── Data/
│   ├── Repositories/
│   ├── Services/
│   └── SignalR/
└── Together.Presentation/        # WPF UI
    ├── Views/
    ├── ViewModels/
    ├── Converters/
    ├── Behaviors/
    └── Controls/
```


## Components and Interfaces

### 1. Domain Layer Components

#### Core Entities

**User Entity**
```csharp
public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string ProfilePictureUrl { get; private set; }
    public string Bio { get; private set; }
    public ProfileVisibility Visibility { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? PartnerId { get; private set; }
    
    // Navigation properties
    public CoupleConnection CoupleConnection { get; private set; }
    public ICollection<Post> Posts { get; private set; }
    public ICollection<FollowRelationship> Following { get; private set; }
    public ICollection<FollowRelationship> Followers { get; private set; }
}
```

**CoupleConnection Entity**
```csharp
public class CoupleConnection
{
    public Guid Id { get; private set; }
    public Guid User1Id { get; private set; }
    public Guid User2Id { get; private set; }
    public DateTime EstablishedAt { get; private set; }
    public DateTime RelationshipStartDate { get; private set; }
    public int LoveStreak { get; private set; }
    public DateTime LastInteractionDate { get; private set; }
    public ConnectionStatus Status { get; private set; }
    
    // Navigation properties
    public User User1 { get; private set; }
    public User User2 { get; private set; }
    public VirtualPet VirtualPet { get; private set; }
    public ICollection<JournalEntry> JournalEntries { get; private set; }
    public ICollection<TodoItem> TodoItems { get; private set; }
    public ICollection<SharedEvent> Events { get; private set; }
}
```

**Post Entity**
```csharp
public class Post
{
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public int LikeCount { get; private set; }
    public int CommentCount { get; private set; }
    
    // Navigation properties
    public User Author { get; private set; }
    public ICollection<PostImage> Images { get; private set; }
    public ICollection<Like> Likes { get; private set; }
    public ICollection<Comment> Comments { get; private set; }
}
```

**MoodEntry Entity**
```csharp
public class MoodEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public MoodType Mood { get; private set; }
    public string Notes { get; private set; }
    public DateTime Timestamp { get; private set; }
    
    public User User { get; private set; }
}
```

#### Value Objects

**Email Value Object**
```csharp
public class Email : ValueObject
{
    public string Value { get; private set; }
    
    private Email(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("Invalid email format");
        Value = value;
    }
    
    public static Email Create(string value) => new Email(value);
    private static bool IsValid(string email) => /* regex validation */;
}
```

#### Domain Interfaces

```csharp
public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<User>> SearchUsersAsync(string query, int limit);
}

public interface ICoupleConnectionRepository
{
    Task<CoupleConnection> GetByUserIdAsync(Guid userId);
    Task<CoupleConnection> GetByIdAsync(Guid id);
    Task AddAsync(CoupleConnection connection);
    Task UpdateAsync(CoupleConnection connection);
    Task DeleteAsync(Guid id);
}

public interface IPostRepository
{
    Task<Post> GetByIdAsync(Guid id);
    Task<IEnumerable<Post>> GetUserPostsAsync(Guid userId, int skip, int take);
    Task<IEnumerable<Post>> GetFeedPostsAsync(Guid userId, int skip, int take);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
}
```

### 2. Application Layer Components

#### Services

**AuthenticationService**
- Handles user registration, login, password reset
- Manages JWT token generation and validation
- Implements password hashing with BCrypt
- Pattern: Service Layer Pattern

```csharp
public interface IAuthenticationService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
```

**CoupleConnectionService**
- Manages couple connection requests and lifecycle
- Enforces one-connection-per-user rule
- Handles connection termination and data archival
- Pattern: Service Layer Pattern

```csharp
public interface ICoupleConnectionService
{
    Task<ConnectionRequest> SendConnectionRequestAsync(Guid fromUserId, Guid toUserId);
    Task<CoupleConnection> AcceptConnectionRequestAsync(Guid requestId);
    Task RejectConnectionRequestAsync(Guid requestId);
    Task TerminateConnectionAsync(Guid connectionId);
    Task<CoupleConnection> GetUserConnectionAsync(Guid userId);
}
```

**MoodAnalysisService**
- Analyzes mood patterns and trends
- Generates supportive message suggestions
- Calculates mood statistics
- Pattern: Strategy Pattern (different analysis strategies)

```csharp
public interface IMoodAnalysisService
{
    Task<MoodTrend> AnalyzeMoodTrendAsync(Guid userId, int days);
    Task<string> GenerateSupportMessageAsync(MoodType mood);
    Task<MoodStatistics> GetMoodStatisticsAsync(Guid userId, DateTime from, DateTime to);
}
```

**LoveStreakService**
- Tracks daily interactions
- Updates streak counters
- Detects milestone achievements
- Pattern: Observer Pattern (notifies on milestones)

```csharp
public interface ILoveStreakService
{
    Task RecordInteractionAsync(Guid connectionId, InteractionType type);
    Task<int> GetCurrentStreakAsync(Guid connectionId);
    Task<bool> CheckAndResetStreakAsync(Guid connectionId);
    Task<IEnumerable<Milestone>> GetAchievedMilestonesAsync(Guid connectionId);
}
```

**ChallengeGeneratorService**
- Generates daily challenges
- Manages challenge completion
- Awards points
- Pattern: Factory Pattern (challenge creation)

```csharp
public interface IChallengeGeneratorService
{
    Task<Challenge> GenerateDailyChallengeAsync(Guid connectionId);
    Task<bool> CompleteChallengeAsync(Guid challengeId, Guid userId);
    Task<IEnumerable<Challenge>> GetActiveChallengesAsync(Guid connectionId);
    Task ArchiveExpiredChallengesAsync();
}
```

**VirtualPetService**
- Manages pet state and evolution
- Calculates experience points
- Handles level-ups and customization
- Pattern: State Pattern (pet mood states)

```csharp
public interface IVirtualPetService
{
    Task<VirtualPet> GetPetAsync(Guid connectionId);
    Task AddExperienceAsync(Guid petId, int points);
    Task UpdatePetStateAsync(Guid petId);
    Task CustomizePetAsync(Guid petId, PetCustomization customization);
}
```

**SocialFeedService**
- Aggregates posts from followed users
- Implements pagination
- Manages feed caching
- Pattern: Repository Pattern

```csharp
public interface ISocialFeedService
{
    Task<FeedResult> GetFeedAsync(Guid userId, int skip, int take);
    Task<IEnumerable<User>> GetSuggestedUsersAsync(Guid userId, int limit);
    Task RefreshFeedCacheAsync(Guid userId);
}
```

**RealTimeSyncService**
- Manages SignalR connections
- Broadcasts updates to connected clients
- Handles reconnection logic
- Pattern: Observer Pattern

```csharp
public interface IRealTimeSyncService
{
    Task BroadcastToPartnerAsync(Guid userId, string eventType, object data);
    Task BroadcastToFollowersAsync(Guid userId, string eventType, object data);
    Task NotifyUserAsync(Guid userId, Notification notification);
}
```

### 3. Infrastructure Layer Components

#### Database Context

```csharp
public class TogetherDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<CoupleConnection> CoupleConnections { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<MoodEntry> MoodEntries { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<SharedEvent> SharedEvents { get; set; }
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<VirtualPet> VirtualPets { get; set; }
    public DbSet<FollowRelationship> FollowRelationships { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TogetherDbContext).Assembly);
    }
}
```

#### Repository Implementations

Repositories implement the interfaces defined in the domain layer and use EF Core for data access.

**Pattern**: Repository Pattern with Unit of Work

#### External Service Integrations

**SupabaseAuthService**
- Integrates with Supabase authentication
- Manages user sessions
- Handles token refresh

**SupabaseStorageService**
- Uploads and retrieves images
- Generates signed URLs for secure access
- Implements image compression

**SignalRHubService**
- Implements SignalR hub for real-time communication
- Manages connection groups (couples, followers)
- Broadcasts events

#### Offline Sync Manager

```csharp
public interface IOfflineSyncManager
{
    Task QueueOperationAsync(OfflineOperation operation);
    Task<bool> IsOnlineAsync();
    Task SyncPendingOperationsAsync();
    Task<T> GetCachedDataAsync<T>(string key);
    Task CacheDataAsync<T>(string key, T data, TimeSpan expiration);
}
```

### 4. Presentation Layer Components

#### ViewModels (MVVM Pattern)

**Base ViewModel**
```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

**MainViewModel**
- Manages navigation between modules
- Holds current user state
- Coordinates module ViewModels
- Pattern: Mediator Pattern

**CoupleHubViewModel**
- Displays dashboard summary
- Shows partner mood and activities
- Manages virtual pet display
- Generates daily suggestions

**JournalViewModel**
- Manages journal entry creation and display
- Handles image attachments
- Implements read status tracking

**MoodTrackerViewModel**
- Provides mood selection interface
- Displays mood history charts
- Shows mood analysis results

**SocialFeedViewModel**
- Displays paginated feed
- Implements infinite scrolling
- Handles real-time post updates
- Pattern: Observer Pattern (subscribes to SignalR events)

**PostCreationViewModel**
- Manages post composition
- Handles image uploads
- Validates content length

#### Commands (Command Pattern)

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;
    
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter) => _execute(parameter);
    
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

#### Navigation Service

```csharp
public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;
    void GoBack();
    bool CanGoBack { get; }
}
```


## Data Models

### Database Schema

#### Users Table
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    profile_picture_url TEXT,
    bio TEXT,
    visibility VARCHAR(20) DEFAULT 'public',
    created_at TIMESTAMP DEFAULT NOW(),
    partner_id UUID REFERENCES users(id),
    CONSTRAINT valid_visibility CHECK (visibility IN ('public', 'friends_only', 'private'))
);
```

#### Couple Connections Table
```sql
CREATE TABLE couple_connections (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user1_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    user2_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    established_at TIMESTAMP DEFAULT NOW(),
    relationship_start_date DATE NOT NULL,
    love_streak INTEGER DEFAULT 0,
    last_interaction_date TIMESTAMP,
    status VARCHAR(20) DEFAULT 'active',
    CONSTRAINT different_users CHECK (user1_id != user2_id),
    CONSTRAINT valid_status CHECK (status IN ('active', 'terminated', 'archived'))
);
```

#### Posts Table
```sql
CREATE TABLE posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    edited_at TIMESTAMP,
    like_count INTEGER DEFAULT 0,
    comment_count INTEGER DEFAULT 0,
    CONSTRAINT content_length CHECK (LENGTH(content) <= 500)
);
```

#### Mood Entries Table
```sql
CREATE TABLE mood_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    mood VARCHAR(20) NOT NULL,
    notes TEXT,
    timestamp TIMESTAMP DEFAULT NOW(),
    CONSTRAINT valid_mood CHECK (mood IN ('happy', 'sad', 'anxious', 'angry', 'excited', 'calm', 'stressed'))
);
```

#### Journal Entries Table
```sql
CREATE TABLE journal_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    connection_id UUID NOT NULL REFERENCES couple_connections(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    is_read_by_partner BOOLEAN DEFAULT FALSE
);
```

#### Todo Items Table
```sql
CREATE TABLE todo_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    connection_id UUID NOT NULL REFERENCES couple_connections(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    assigned_to UUID REFERENCES users(id),
    created_by UUID NOT NULL REFERENCES users(id),
    due_date TIMESTAMP,
    completed BOOLEAN DEFAULT FALSE,
    completed_at TIMESTAMP,
    tags TEXT[],
    created_at TIMESTAMP DEFAULT NOW()
);
```

#### Shared Events Table
```sql
CREATE TABLE shared_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    connection_id UUID NOT NULL REFERENCES couple_connections(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    event_date TIMESTAMP NOT NULL,
    recurrence VARCHAR(20),
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT valid_recurrence CHECK (recurrence IN ('none', 'daily', 'weekly', 'monthly', 'yearly'))
);
```

#### Virtual Pets Table
```sql
CREATE TABLE virtual_pets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    connection_id UUID UNIQUE NOT NULL REFERENCES couple_connections(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    level INTEGER DEFAULT 1,
    experience_points INTEGER DEFAULT 0,
    appearance_options JSONB,
    state VARCHAR(20) DEFAULT 'happy',
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT valid_state CHECK (state IN ('happy', 'sad', 'neglected', 'excited'))
);
```

#### Challenges Table
```sql
CREATE TABLE challenges (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    connection_id UUID NOT NULL REFERENCES couple_connections(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    category VARCHAR(50) NOT NULL,
    points INTEGER DEFAULT 10,
    expires_at TIMESTAMP NOT NULL,
    completed_by_user1 BOOLEAN DEFAULT FALSE,
    completed_by_user2 BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT valid_category CHECK (category IN ('communication', 'fun', 'appreciation', 'learning'))
);
```

#### Follow Relationships Table
```sql
CREATE TABLE follow_relationships (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    follower_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    following_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT NOW(),
    accepted_at TIMESTAMP,
    CONSTRAINT different_users CHECK (follower_id != following_id),
    CONSTRAINT valid_status CHECK (status IN ('pending', 'accepted', 'rejected')),
    UNIQUE(follower_id, following_id)
);
```

#### Likes Table
```sql
CREATE TABLE likes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(post_id, user_id)
);
```

#### Comments Table
```sql
CREATE TABLE comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT content_length CHECK (LENGTH(content) <= 300)
);
```

### DTOs (Data Transfer Objects)

```csharp
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResult(bool Success, string Token, string Message, UserDto User);
public record UserDto(Guid Id, string Username, string Email, string ProfilePictureUrl, string Bio);
public record PostDto(Guid Id, UserDto Author, string Content, DateTime CreatedAt, int LikeCount, int CommentCount, List<string> ImageUrls);
public record MoodEntryDto(Guid Id, string Mood, string Notes, DateTime Timestamp);
public record JournalEntryDto(Guid Id, UserDto Author, string Content, DateTime CreatedAt, bool IsReadByPartner);
public record ChallengeDto(Guid Id, string Title, string Description, string Category, int Points, DateTime ExpiresAt, bool CompletedByUser1, bool CompletedByUser2);
```

### Enums

```csharp
public enum MoodType
{
    Happy,
    Sad,
    Anxious,
    Angry,
    Excited,
    Calm,
    Stressed
}

public enum ProfileVisibility
{
    Public,
    FriendsOnly,
    Private
}

public enum ConnectionStatus
{
    Active,
    Terminated,
    Archived
}

public enum InteractionType
{
    JournalEntry,
    MoodLog,
    ChatMessage,
    ChallengeCompletion,
    TodoCompletion
}

public enum PetState
{
    Happy,
    Sad,
    Neglected,
    Excited
}
```

## Error Handling

### Exception Hierarchy

```csharp
public class TogetherException : Exception
{
    public TogetherException(string message) : base(message) { }
    public TogetherException(string message, Exception inner) : base(message, inner) { }
}

public class AuthenticationException : TogetherException
{
    public AuthenticationException(string message) : base(message) { }
}

public class ValidationException : TogetherException
{
    public Dictionary<string, string[]> Errors { get; }
    public ValidationException(Dictionary<string, string[]> errors) 
        : base("Validation failed")
    {
        Errors = errors;
    }
}

public class NotFoundException : TogetherException
{
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key {key} not found") { }
}

public class BusinessRuleViolationException : TogetherException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}
```

### Error Handling Strategy

**Service Layer**
- Services throw domain-specific exceptions
- Validation occurs before business logic execution
- All exceptions are logged with context

**Application Layer**
- Catches service exceptions
- Transforms to appropriate response DTOs
- Returns Result<T> pattern for operations

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string ErrorMessage { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }
    
    public static Result<T> Success(T data) => new Result<T>(true, data, null, null);
    public static Result<T> Failure(string error) => new Result<T>(false, default, error, null);
    public static Result<T> ValidationFailure(Dictionary<string, string[]> errors) 
        => new Result<T>(false, default, "Validation failed", errors);
}
```

**Presentation Layer**
- ViewModels catch exceptions from services
- Display user-friendly error messages
- Log errors for debugging
- Show validation errors inline with form fields

### Logging Strategy

```csharp
public interface ILogger
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogDebug(string message, params object[] args);
}
```

- Use Serilog for structured logging
- Log to file and console
- Include correlation IDs for request tracking
- Sensitive data (passwords, tokens) never logged

## Testing Strategy

### Unit Testing

**Domain Layer**
- Test entity business logic
- Test value object validation
- Test domain events
- Framework: xUnit
- Mocking: Moq

**Application Layer**
- Test service methods in isolation
- Mock repository dependencies
- Test business rule enforcement
- Verify correct exception handling

**Example Test Structure**
```csharp
public class MoodAnalysisServiceTests
{
    private readonly Mock<IMoodEntryRepository> _mockRepository;
    private readonly MoodAnalysisService _service;
    
    public MoodAnalysisServiceTests()
    {
        _mockRepository = new Mock<IMoodEntryRepository>();
        _service = new MoodAnalysisService(_mockRepository.Object);
    }
    
    [Fact]
    public async Task AnalyzeMoodTrend_WithPositiveMoods_ReturnsPositiveTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var moods = CreatePositiveMoodEntries(userId, 7);
        _mockRepository.Setup(r => r.GetUserMoodsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(moods);
        
        // Act
        var result = await _service.AnalyzeMoodTrendAsync(userId, 7);
        
        // Assert
        Assert.Equal(TrendType.Positive, result.TrendType);
    }
}
```

### Integration Testing

**Infrastructure Layer**
- Test repository implementations with real database (test container)
- Test Supabase integration
- Test SignalR hub functionality
- Framework: xUnit with Testcontainers

**API Integration**
- Test end-to-end flows
- Verify data persistence
- Test real-time synchronization

### UI Testing

**ViewModel Testing**
- Test command execution
- Test property change notifications
- Test navigation logic
- Mock service dependencies

**Manual Testing**
- User acceptance testing for UI/UX
- Cross-module integration testing
- Performance testing under load

### Test Coverage Goals

- Domain Layer: 90%+ coverage
- Application Layer: 85%+ coverage
- Infrastructure Layer: 70%+ coverage
- Presentation Layer: 60%+ coverage (ViewModels)

## Security Considerations

### Authentication & Authorization

- JWT tokens with 24-hour expiration
- Refresh tokens stored securely
- Password hashing with BCrypt (10+ rounds)
- Email verification for new accounts
- Rate limiting on authentication endpoints

### Data Protection

- TLS 1.2+ for all network communication
- Encrypted storage for sensitive local data
- Secure token storage using Windows Credential Manager
- Input validation and sanitization
- SQL injection prevention via parameterized queries

### Privacy

- Couple data isolated by connection ID
- Profile visibility controls enforced at query level
- User data deletion compliance (GDPR)
- No tracking of user location without explicit consent
- Audit logging for sensitive operations

## Performance Optimization

### Caching Strategy

**Local Cache (SQLite)**
- Recent posts (last 100)
- User profile data
- Couple connection data (30 days)
- Mood history (30 days)
- Cache invalidation on updates

**In-Memory Cache**
- Current user session data
- Frequently accessed reference data
- Cache expiration: 5-15 minutes

### Database Optimization

- Indexes on foreign keys
- Composite indexes for common queries
- Pagination for large result sets
- Lazy loading for navigation properties
- Connection pooling

### UI Performance

- Virtualization for long lists (VirtualizingStackPanel)
- Image lazy loading and caching
- Async operations for all I/O
- Background threads for heavy computations
- Debouncing for search inputs

### Real-time Optimization

- SignalR connection pooling
- Message batching for bulk updates
- Selective subscription to relevant channels
- Automatic reconnection with exponential backoff

## Deployment Considerations

### Application Packaging

- ClickOnce deployment for easy updates
- Self-contained .NET runtime
- Automatic update checks on startup
- Rollback capability for failed updates

### Configuration Management

- appsettings.json for environment-specific settings
- User settings stored in AppData
- Secure storage for API keys and secrets
- Environment variables for sensitive configuration

### Monitoring & Diagnostics

- Application Insights for telemetry
- Error reporting to centralized service
- Performance metrics collection
- User analytics (opt-in)

## Future Extensibility

### Plugin Architecture

- Interface-based plugin system
- Dynamic module loading
- Custom challenge types
- Theme extensions

### API Exposure

- REST API for mobile app integration
- Webhook support for external integrations
- OAuth2 for third-party authentication

### Scalability Considerations

- Microservices migration path
- Message queue for async operations
- CDN for static assets
- Database sharding strategy
