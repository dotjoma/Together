using Together.Application.DTOs;
using Together.Domain.Enums;

namespace Together.Application.Interfaces;

public interface IMoodAnalysisService
{
    Task<MoodTrendDto> AnalyzeMoodTrendAsync(Guid userId, int days = 30);
    Task<string> GenerateSupportMessageAsync(MoodType mood);
}
