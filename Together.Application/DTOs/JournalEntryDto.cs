namespace Together.Application.DTOs;

public record JournalEntryDto(
    Guid Id,
    Guid ConnectionId,
    UserDto Author,
    string Content,
    DateTime CreatedAt,
    bool IsReadByPartner,
    string? ImageUrl
);
