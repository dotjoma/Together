namespace Together.Application.DTOs;

public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Message,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime CreatedAt
);
