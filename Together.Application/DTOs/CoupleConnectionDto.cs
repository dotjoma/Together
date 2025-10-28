namespace Together.Application.DTOs;

public record CoupleConnectionDto(
    Guid Id,
    UserDto User1,
    UserDto User2,
    DateTime EstablishedAt,
    DateTime RelationshipStartDate,
    int LoveStreak,
    DateTime? LastInteractionDate,
    string Status
);
