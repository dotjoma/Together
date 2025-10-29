using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IMoodTrackingService _moodTrackingService;
    private readonly ILoveStreakService _loveStreakService;
    private readonly IEventService _eventService;
    private readonly IVirtualPetService _virtualPetService;
    private readonly IJournalService _journalService;
    private readonly ITodoService _todoService;
    private readonly IMoodAnalysisService _moodAnalysisService;
    private readonly IUserRepository _userRepository;
    private readonly IMoodEntryRepository _moodEntryRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly ITodoItemRepository _todoItemRepository;

    // Suggestion pools
    private static readonly List<DailySuggestionDto> PositiveActivities = new()
    {
        new("Fun", "Share a Laugh", "Send your partner a funny meme or joke that reminds you of them"),
        new("Appreciation", "Express Gratitude", "Tell your partner one thing you appreciate about them today"),
        new("Communication", "Deep Question", "Ask your partner: What's one dream you've never told anyone about?"),
        new("Fun", "Virtual Date", "Plan a virtual movie night or game session together"),
        new("Learning", "Share Knowledge", "Teach your partner something new you learned recently"),
        new("Appreciation", "Compliment", "Give your partner a genuine compliment about their personality"),
        new("Communication", "Memory Lane", "Share your favorite memory together from the past month"),
        new("Fun", "Challenge Time", "Create a fun challenge for both of you to complete today"),
        new("Appreciation", "Love Note", "Write a short love note in your shared journal"),
        new("Communication", "Future Plans", "Discuss one thing you're both looking forward to"),
        new("Fun", "Photo Share", "Share a photo that made you think of your partner today"),
        new("Learning", "Book Recommendation", "Recommend a book, article, or podcast to each other"),
        new("Appreciation", "Acknowledge Effort", "Thank your partner for something they did recently"),
        new("Communication", "Feelings Check", "Ask your partner how they're really feeling today"),
        new("Fun", "Music Moment", "Share a song that describes your current mood")
    };

    private static readonly List<string> ConversationStarters = new()
    {
        "If you could relive one day from our relationship, which would it be?",
        "What's something new you'd like to try together?",
        "What made you smile today?",
        "If we could travel anywhere right now, where would you want to go?",
        "What's one thing I do that makes you feel loved?",
        "What's your favorite thing about us?",
        "What's a goal you have for this month?",
        "What's something you're proud of recently?",
        "If you could have any superpower, what would it be and why?",
        "What's your ideal way to spend a weekend together?",
        "What's something you've been wanting to tell me?",
        "What's a small thing that made your day better?",
        "What's one thing you're grateful for today?",
        "What's your favorite memory of us from this year?",
        "What's something you'd like to learn together?"
    };

    public DashboardService(
        ICoupleConnectionRepository connectionRepository,
        IMoodTrackingService moodTrackingService,
        ILoveStreakService loveStreakService,
        IEventService eventService,
        IVirtualPetService virtualPetService,
        IJournalService journalService,
        ITodoService todoService,
        IMoodAnalysisService moodAnalysisService,
        IUserRepository userRepository,
        IMoodEntryRepository moodEntryRepository,
        IJournalEntryRepository journalEntryRepository,
        ITodoItemRepository todoItemRepository)
    {
        _connectionRepository = connectionRepository;
        _moodTrackingService = moodTrackingService;
        _loveStreakService = loveStreakService;
        _eventService = eventService;
        _virtualPetService = virtualPetService;
        _journalService = journalService;
        _todoService = todoService;
        _moodAnalysisService = moodAnalysisService;
        _userRepository = userRepository;
        _moodEntryRepository = moodEntryRepository;
        _journalEntryRepository = journalEntryRepository;
        _todoItemRepository = todoItemRepository;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        // Get user's couple connection
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            throw new BusinessRuleViolationException("User must have an active couple connection to view dashboard");
        }

        // Determine partner ID
        var partnerId = connection.User1Id == userId ? connection.User2Id : connection.User1Id;

        // Get partner's latest mood
        var partnerMood = await _moodTrackingService.GetLatestMoodAsync(partnerId);

        // Get love streak
        var loveStreak = await _loveStreakService.GetCurrentStreakAsync(connection.Id);

        // Get upcoming events (next 5)
        var upcomingEvents = await _eventService.GetUpcomingEventsAsync(userId, 5);

        // Get virtual pet
        var virtualPet = await _virtualPetService.GetPetAsync(connection.Id);

        // Get days together
        var daysTogether = await _eventService.GetDaysTogetherAsync(userId);

        // Generate supportive message if partner mood is negative
        string? supportiveMessage = null;
        if (partnerMood != null && IsNegativeMood(partnerMood.Mood))
        {
            if (Enum.TryParse<MoodType>(partnerMood.Mood, true, out var moodType))
            {
                supportiveMessage = await _moodAnalysisService.GenerateSupportMessageAsync(moodType);
            }
        }

        return new DashboardSummaryDto(
            partnerMood,
            loveStreak,
            upcomingEvents,
            virtualPet,
            daysTogether,
            supportiveMessage
        );
    }

    public async Task<IEnumerable<TogetherMomentDto>> GetTogetherMomentsAsync(Guid userId, int limit = 5)
    {
        // Get user's couple connection
        var connection = await _connectionRepository.GetByUserIdAsync(userId);
        if (connection == null)
        {
            return Enumerable.Empty<TogetherMomentDto>();
        }

        var moments = new List<TogetherMomentDto>();

        // Get recent journal entries
        var journalEntries = await _journalEntryRepository.GetByConnectionIdAsync(connection.Id);
        foreach (var entry in journalEntries.OrderByDescending(e => e.CreatedAt).Take(limit))
        {
            var author = entry.Author ?? await _userRepository.GetByIdAsync(entry.AuthorId);
            if (author != null)
            {
                var authorDto = new UserDto(
                    author.Id,
                    author.Username,
                    author.Email.Value,
                    author.ProfilePictureUrl,
                    author.Bio
                );

                var contentPreview = entry.Content?.Length > 100 
                    ? entry.Content.Substring(0, 100) + "..." 
                    : entry.Content ?? string.Empty;

                moments.Add(new TogetherMomentDto(
                    entry.Id,
                    "JournalEntry",
                    $"{author.Username} wrote: {contentPreview}",
                    authorDto,
                    entry.CreatedAt
                ));
            }
        }

        // Get recent mood entries
        var user1Moods = await _moodEntryRepository.GetUserMoodsAsync(connection.User1Id, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        var user2Moods = await _moodEntryRepository.GetUserMoodsAsync(connection.User2Id, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        var allMoods = user1Moods.Concat(user2Moods).OrderByDescending(m => m.Timestamp).Take(limit);

        foreach (var mood in allMoods)
        {
            var user = await _userRepository.GetByIdAsync(mood.UserId);
            if (user != null)
            {
                var userDto = new UserDto(
                    user.Id,
                    user.Username,
                    user.Email.Value,
                    user.ProfilePictureUrl,
                    user.Bio
                );

                moments.Add(new TogetherMomentDto(
                    mood.Id,
                    "MoodUpdate",
                    $"{user.Username} is feeling {mood.Mood.ToString().ToLower()}",
                    userDto,
                    mood.Timestamp
                ));
            }
        }

        // Get recent completed todos
        var todos = await _todoItemRepository.GetByConnectionIdAsync(connection.Id);
        var completedTodos = todos.Where(t => t.Completed).OrderByDescending(t => t.CompletedAt).Take(limit);

        foreach (var todo in completedTodos)
        {
            if (todo.CompletedAt.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(todo.AssignedTo ?? todo.CreatedBy);
                if (user != null)
                {
                    var userDto = new UserDto(
                        user.Id,
                        user.Username,
                        user.Email.Value,
                        user.ProfilePictureUrl,
                        user.Bio
                    );

                    moments.Add(new TogetherMomentDto(
                        todo.Id,
                        "TodoCompleted",
                        $"{user.Username} completed: {todo.Title}",
                        userDto,
                        todo.CompletedAt.Value
                    ));
                }
            }
        }

        // Sort all moments by timestamp and return top 'limit'
        return moments
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToList();
    }

    public Task<DailySuggestionDto> GetDailySuggestionAsync(Guid userId)
    {
        // Use user ID and current date as seed for consistent daily suggestions
        var seed = userId.GetHashCode() + DateTime.UtcNow.Date.GetHashCode();
        var random = new Random(seed);

        // Randomly choose between positive activity and conversation starter
        var suggestionType = random.Next(2);

        if (suggestionType == 0)
        {
            // Return a positive activity
            var index = random.Next(PositiveActivities.Count);
            return Task.FromResult(PositiveActivities[index]);
        }
        else
        {
            // Return a conversation starter
            var index = random.Next(ConversationStarters.Count);
            return Task.FromResult(new DailySuggestionDto(
                "Communication",
                "Conversation Starter",
                ConversationStarters[index]
            ));
        }
    }

    private bool IsNegativeMood(string mood)
    {
        return mood.Equals("Sad", StringComparison.OrdinalIgnoreCase) ||
               mood.Equals("Anxious", StringComparison.OrdinalIgnoreCase) ||
               mood.Equals("Angry", StringComparison.OrdinalIgnoreCase) ||
               mood.Equals("Stressed", StringComparison.OrdinalIgnoreCase);
    }
}
