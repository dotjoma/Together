using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<JournalEntry>> GetByConnectionIdAsync(Guid connectionId);
    Task AddAsync(JournalEntry entry);
    Task UpdateAsync(JournalEntry entry);
    Task DeleteAsync(Guid id);
}
