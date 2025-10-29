namespace Together.Application.DTOs;

public record DashboardSummaryDto(
    MoodEntryDto? PartnerMood,
    int LoveStreak,
    IEnumerable<SharedEventDto> UpcomingEvents,
    VirtualPetDto? VirtualPet,
    int DaysTogether,
    string? SupportiveMessage
);
