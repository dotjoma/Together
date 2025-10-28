using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IMoodTrackingService
{
    Task<MoodEntryDto> CreateMoodEntryAsync(Guid userId, CreateMoodEntryDto dto);
    Task<IEnumerable<MoodEntryDto>> GetMoodHistoryAsync(Guid userId, int days = 30);
    Task<MoodEntryDto?> GetLatestMoodAsync(Guid userId);
}
