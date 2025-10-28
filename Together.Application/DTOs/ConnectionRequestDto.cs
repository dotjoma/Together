namespace Together.Application.DTOs;

public record ConnectionRequestDto(
    Guid Id,
    UserDto FromUser,
    UserDto ToUser,
    DateTime CreatedAt,
    string Status
);
