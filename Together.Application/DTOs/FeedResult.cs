namespace Together.Application.DTOs;

public record FeedResult(
    IEnumerable<PostDto> Posts,
    int TotalCount,
    bool HasMore
);
