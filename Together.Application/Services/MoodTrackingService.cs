using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class MoodTrackingService : IMoodTrackingService
{
    private readonly IMoodEntryRepository _moodEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICoupleConnectionRepository _coupleConnectionRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMoodAnalysisService _moodAnalysisService;

    public MoodTrackingService(
        IMoodEntryRepository moodEntryRepository,
        IUserRepository userRepository,
        ICoupleConnectionRepository coupleConnectionRepository,
        INotificationRepository notificationRepository,
        IMoodAnalysisService moodAnalysisService)
    {
        _moodEntryRepository = moodEntryRepository;
        _userRepository = userRepository;
        _coupleConnectionRepository = coupleConnectionRepository;
        _notificationRepository = notificationRepository;
        _moodAnalysisService = moodAnalysisService;
    }

    public async Task<MoodEntryDto> CreateMoodEntryAsync(Guid userId, CreateMoodEntryDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        // Parse mood type
        if (!Enum.TryParse<MoodType>(dto.Mood, true, out var moodType))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(dto.Mood), new[] { "Invalid mood type" } }
            });
        }

        // Create mood entry
        var moodEntry = new MoodEntry(userId, moodType, dto.Notes);
        await _moodEntryRepository.AddAsync(moodEntry);

        // Check if user has a partner and notify them if mood is negative
        var connection = await _coupleConnectionRepository.GetByUserIdAsync(userId);
        if (connection != null && IsNegativeMood(moodType))
        {
            var partnerId = connection.User1Id == userId ? connection.User2Id : connection.User1Id;
            var supportMessage = await _moodAnalysisService.GenerateSupportMessageAsync(moodType);

            var notification = new Notification(
                partnerId,
                "MoodUpdate",
                $"Your partner is feeling {moodType.ToString().ToLower()}. {supportMessage}",
                moodEntry.Id
            );

            await _notificationRepository.AddAsync(notification);
        }

        return new MoodEntryDto(
            moodEntry.Id,
            moodEntry.Mood.ToString(),
            moodEntry.Notes,
            moodEntry.Timestamp
        );
    }

    public async Task<IEnumerable<MoodEntryDto>> GetMoodHistoryAsync(Guid userId, int days = 30)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);
        var toDate = DateTime.UtcNow;

        var moodEntries = await _moodEntryRepository.GetUserMoodsAsync(userId, fromDate, toDate);

        return moodEntries.Select(m => new MoodEntryDto(
            m.Id,
            m.Mood.ToString(),
            m.Notes,
            m.Timestamp
        ));
    }

    public async Task<MoodEntryDto?> GetLatestMoodAsync(Guid userId)
    {
        var moodEntry = await _moodEntryRepository.GetLatestMoodAsync(userId);

        if (moodEntry == null)
        {
            return null;
        }

        return new MoodEntryDto(
            moodEntry.Id,
            moodEntry.Mood.ToString(),
            moodEntry.Notes,
            moodEntry.Timestamp
        );
    }

    private bool IsNegativeMood(MoodType mood)
    {
        return mood == MoodType.Sad || mood == MoodType.Anxious || mood == MoodType.Angry;
    }
}
