namespace Together.Application.DTOs;

/// <summary>
/// Data transfer object for journal entries
/// </summary>
public record JournalEntryDto(
    Guid Id,
    Guid ConnectionId,
    UserDto Author,
    string Content,
    DateTime CreatedAt,
    bool IsReadByPartner,
    string? ImageUrl
);
