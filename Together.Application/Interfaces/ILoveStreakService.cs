using Together.Domain.Enums;

namespace Together.Application.Interfaces;

public interface ILoveStreakService
{
    Task RecordInteractionAsync(Guid connectionId, InteractionType interactionType);
    Task<int> GetCurrentStreakAsync(Guid connectionId);
    Task CheckAndResetStreakAsync(Guid connectionId);
    Task<IEnumerable<int>> GetStreakMilestonesAsync(Guid connectionId);
}
