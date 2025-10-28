namespace Together.Application.DTOs;

public record ChallengeDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    int Points,
    DateTime ExpiresAt,
    bool CompletedByUser1,
    bool CompletedByUser2,
    DateTime CreatedAt,
    bool IsFullyCompleted,
    bool IsExpired
);
