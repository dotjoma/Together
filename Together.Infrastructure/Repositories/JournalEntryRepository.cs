using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly TogetherDbContext _context;

    public JournalEntryRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id)
    {
        return await _context.JournalEntries
            .Include(j => j.Author)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JournalEntry>> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _context.JournalEntries
            .Include(j => j.Author)
            .Where(j => j.ConnectionId == connectionId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(JournalEntry entry)
    {
        await _context.JournalEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(JournalEntry entry)
    {
        _context.JournalEntries.Update(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry != null)
        {
            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
