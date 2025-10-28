using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class ChallengeService : IChallengeService
{
    private readonly IChallengeRepository _challengeRepository;
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IUserRepository _userRepository;

    public ChallengeService(
        IChallengeRepository challengeRepository,
        ICoupleConnectionRepository connectionRepository,
        IUserRepository userRepository)
    {
        _challengeRepository = challengeRepository;
        _connectionRepository = connectionRepository;
        _userRepository = userRepository;
    }

    public async Task<ChallengeDto> GenerateDailyChallengeAsync(Guid connectionId)
    {
        // Check if connection exists
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        // Check if today's challenge already exists
        var existingChallenge = await _challengeRepository.GetTodaysChallengeAsync(connectionId);
        if (existingChallenge != null)
            return MapToDto(existingChallenge);

        // Generate new challenge using factory
        var challenge = ChallengeFactory.CreateRandomChallenge(connectionId);
        
        await _challengeRepository.AddAsync(challenge);

        return MapToDto(challenge);
    }

    public async Task<ChallengeDto> CompleteChallengeAsync(Guid challengeId, Guid userId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        
        if (challenge.IsExpired())
            throw new BusinessRuleViolationException("Cannot complete an expired challenge");

        // Determine if user is User1 or User2 in the connection
        var connection = await _connectionRepository.GetByIdAsync(challenge.ConnectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), challenge.ConnectionId);

        bool isUser1 = connection.User1Id == userId;
        bool isUser2 = connection.User2Id == userId;

        if (!isUser1 && !isUser2)
            throw new BusinessRuleViolationException("User is not part of this couple connection");

        // Mark as completed
        challenge.MarkCompletedByUser(isUser1);
        await _challengeRepository.UpdateAsync(challenge);

        return MapToDto(challenge);
    }

    public async Task<IEnumerable<ChallengeDto>> GetActiveChallengesAsync(Guid connectionId)
    {
        var challenges = await _challengeRepository.GetActiveChallengesAsync(connectionId);
        return challenges.Select(MapToDto);
    }

    public async Task<int> GetCoupleScoreAsync(Guid connectionId)
    {
        return await _challengeRepository.GetCoupleScoreAsync(connectionId);
    }

    public async Task ArchiveExpiredChallengesAsync()
    {
        var expiredChallenges = await _challengeRepository.GetExpiredChallengesAsync();
        
        foreach (var challenge in expiredChallenges)
        {
            await _challengeRepository.DeleteAsync(challenge.Id);
        }
    }

    private ChallengeDto MapToDto(Challenge challenge)
    {
        return new ChallengeDto(
            challenge.Id,
            challenge.Title ?? string.Empty,
            challenge.Description ?? string.Empty,
            challenge.Category ?? string.Empty,
            challenge.Points,
            challenge.ExpiresAt,
            challenge.CompletedByUser1,
            challenge.CompletedByUser2,
            challenge.CreatedAt,
            challenge.IsFullyCompleted(),
            challenge.IsExpired()
        );
    }
}

// Challenge Factory with predefined challenges by category
public static class ChallengeFactory
{
    private static readonly Random _random = new Random();

    private static readonly Dictionary<string, List<(string Title, string Description, int Points)>> _challengeTemplates = new()
    {
        ["communication"] = new List<(string, string, int)>
        {
            ("Share Your Day", "Take 15 minutes to share the highlights and lowlights of your day with each other", 10),
            ("Ask Deep Questions", "Ask each other 3 meaningful questions you've never asked before", 15),
            ("Express Gratitude", "Tell your partner 3 specific things you appreciate about them today", 10),
            ("Active Listening", "Practice active listening: one person shares for 5 minutes while the other listens without interrupting", 15),
            ("Future Dreams", "Discuss one dream or goal you each have for your future together", 20),
            ("Love Language", "Identify and discuss each other's primary love language", 15),
            ("Conflict Resolution", "Discuss a small disagreement and practice resolving it calmly together", 20),
            ("Compliment Exchange", "Give each other 5 genuine compliments", 10)
        },
        ["fun"] = new List<(string, string, int)>
        {
            ("Dance Party", "Put on your favorite song and dance together for at least one full song", 10),
            ("Cook Together", "Prepare a meal or snack together, trying a new recipe", 15),
            ("Game Night", "Play a board game, card game, or video game together", 10),
            ("Photo Challenge", "Take 5 silly selfies together in different poses", 10),
            ("Movie Night", "Watch a movie neither of you has seen before", 10),
            ("Karaoke Duet", "Sing a duet together (even if it's just in your living room)", 15),
            ("Build Something", "Work together to build or create something (puzzle, craft, LEGO, etc.)", 15),
            ("Outdoor Adventure", "Go for a walk, hike, or explore a new place together", 20)
        },
        ["appreciation"] = new List<(string, string, int)>
        {
            ("Love Letter", "Write a short love letter or note to your partner", 20),
            ("Memory Lane", "Share your favorite memory together from the past month", 15),
            ("Surprise Gesture", "Do one small unexpected thing to make your partner smile", 15),
            ("Quality Time", "Spend 30 minutes of uninterrupted quality time together (no phones)", 20),
            ("Breakfast in Bed", "Surprise your partner with breakfast or their favorite drink", 15),
            ("Massage Exchange", "Give each other a 10-minute shoulder or foot massage", 15),
            ("Playlist Gift", "Create a playlist of songs that remind you of your partner", 10),
            ("Affirmation Shower", "Spend 5 minutes telling your partner why they're amazing", 15)
        },
        ["learning"] = new List<(string, string, int)>
        {
            ("Teach Me Something", "Teach each other something new - a skill, fact, or hobby", 20),
            ("Read Together", "Read the same article or book chapter and discuss it", 15),
            ("Learn a Word", "Learn a new word in a foreign language together", 10),
            ("Documentary Date", "Watch an educational documentary together", 15),
            ("Relationship Quiz", "Take an online relationship quiz or compatibility test together", 10),
            ("Goal Setting", "Set one personal goal and one relationship goal for the month", 20),
            ("Cultural Exchange", "Share something about your family traditions or cultural background", 15),
            ("TED Talk Discussion", "Watch a TED talk together and discuss your thoughts", 15)
        }
    };

    public static Challenge CreateRandomChallenge(Guid connectionId)
    {
        // Select random category
        var categories = _challengeTemplates.Keys.ToList();
        var category = categories[_random.Next(categories.Count)];

        // Select random challenge from category
        var templates = _challengeTemplates[category];
        var template = templates[_random.Next(templates.Count)];

        return new Challenge(
            connectionId,
            template.Title,
            template.Description,
            category,
            template.Points,
            DateTime.UtcNow.AddHours(24)
        );
    }

    public static Challenge CreateChallengeByCategory(Guid connectionId, string category)
    {
        if (!_challengeTemplates.ContainsKey(category.ToLower()))
            throw new ArgumentException($"Invalid category: {category}");

        var templates = _challengeTemplates[category.ToLower()];
        var template = templates[_random.Next(templates.Count)];

        return new Challenge(
            connectionId,
            template.Title,
            template.Description,
            category.ToLower(),
            template.Points,
            DateTime.UtcNow.AddHours(24)
        );
    }
}
