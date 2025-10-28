namespace Together.Application.DTOs;

public record MoodTrendDto(
    string TrendType,
    double AverageScore,
    Dictionary<string, int> MoodDistribution,
    List<MoodEntryDto> RecentEntries
);
