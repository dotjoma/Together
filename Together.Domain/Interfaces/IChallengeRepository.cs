using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IChallengeRepository
{
    Task<Challenge> GetByIdAsync(Guid id);
    Task<IEnumerable<Challenge>> GetActiveChallengesAsync(Guid connectionId);
    Task<IEnumerable<Challenge>> GetExpiredChallengesAsync();
    Task<Challenge?> GetTodaysChallengeAsync(Guid connectionId);
    Task AddAsync(Challenge challenge);
    Task UpdateAsync(Challenge challenge);
    Task DeleteAsync(Guid id);
    Task<int> GetCoupleScoreAsync(Guid connectionId);
}
