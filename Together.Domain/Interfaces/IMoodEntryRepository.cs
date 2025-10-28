using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IMoodEntryRepository
{
    Task<MoodEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<MoodEntry>> GetUserMoodsAsync(Guid userId, DateTime from, DateTime to);
    Task<MoodEntry?> GetLatestMoodAsync(Guid userId);
    Task AddAsync(MoodEntry moodEntry);
    Task UpdateAsync(MoodEntry moodEntry);
    Task DeleteAsync(Guid id);
}
