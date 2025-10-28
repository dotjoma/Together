namespace Together.Application.DTOs;

public record StreakMilestoneDto(
    int Days,
    string Title,
    string Message,
    DateTime AchievedAt
);
