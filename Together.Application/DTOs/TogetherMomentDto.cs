namespace Together.Application.DTOs;

public record TogetherMomentDto(
    Guid Id,
    string ActivityType,
    string Description,
    UserDto User,
    DateTime Timestamp
);
