namespace Together.Application.DTOs;

public record CreatePostDto(
    string Content,
    List<string>? ImagePaths = null
);
