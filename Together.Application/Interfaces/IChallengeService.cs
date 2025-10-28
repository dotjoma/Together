using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IChallengeService
{
    Task<ChallengeDto> GenerateDailyChallengeAsync(Guid connectionId);
    Task<ChallengeDto> CompleteChallengeAsync(Guid challengeId, Guid userId);
    Task<IEnumerable<ChallengeDto>> GetActiveChallengesAsync(Guid connectionId);
    Task<int> GetCoupleScoreAsync(Guid connectionId);
    Task ArchiveExpiredChallengesAsync();
}
