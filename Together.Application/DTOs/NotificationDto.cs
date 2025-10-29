namespace Together.Application.DTOs;

/// <summary>
/// Data transfer object for notifications
/// </summary>
public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Message,
    DateTime CreatedAt,
    bool IsRead
);
