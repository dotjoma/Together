namespace Together.Application.DTOs;

public record PostDto(
    Guid Id,
    UserDto Author,
    string Content,
    DateTime CreatedAt,
    DateTime? EditedAt,
    int LikeCount,
    int CommentCount,
    List<string> ImageUrls
);
