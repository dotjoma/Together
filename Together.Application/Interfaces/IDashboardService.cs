using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
    Task<IEnumerable<TogetherMomentDto>> GetTogetherMomentsAsync(Guid userId, int limit = 5);
    Task<DailySuggestionDto> GetDailySuggestionAsync(Guid userId);
}
