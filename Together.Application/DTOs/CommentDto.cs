namespace Together.Application.DTOs;

public record CommentDto(
    Guid Id,
    Guid PostId,
    UserDto Author,
    string Content,
    DateTime CreatedAt
);
