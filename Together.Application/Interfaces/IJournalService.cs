using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IJournalService
{
    Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryDto dto);
    Task<IEnumerable<JournalEntryDto>> GetJournalEntriesAsync(Guid connectionId);
    Task MarkAsReadAsync(Guid entryId, Guid userId);
    Task DeleteJournalEntryAsync(Guid entryId, Guid userId);
    Task<string?> UploadImageAsync(Guid userId, Stream imageStream, string fileName);
}
