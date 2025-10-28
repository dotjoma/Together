namespace Together.Application.DTOs;

public record CreateJournalEntryDto(
    Guid ConnectionId,
    Guid AuthorId,
    string Content,
    string? ImageUrl = null
);
