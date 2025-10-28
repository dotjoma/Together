namespace Together.Application.DTOs;

public record UpdatePostDto(
    Guid PostId,
    string Content
);
