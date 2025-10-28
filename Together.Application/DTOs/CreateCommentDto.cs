namespace Together.Application.DTOs;

public record CreateCommentDto(
    Guid PostId,
    string Content
);
