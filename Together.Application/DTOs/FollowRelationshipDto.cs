namespace Together.Application.DTOs;

public record FollowRelationshipDto(
    Guid Id,
    UserDto Follower,
    UserDto Following,
    string Status,
    DateTime CreatedAt,
    DateTime? AcceptedAt
);
