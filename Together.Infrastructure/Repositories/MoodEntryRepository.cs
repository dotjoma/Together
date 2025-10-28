using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class MoodEntryRepository : IMoodEntryRepository
{
    private readonly TogetherDbContext _context;

    public MoodEntryRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<MoodEntry?> GetByIdAsync(Guid id)
    {
        return await _context.MoodEntries
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MoodEntry>> GetUserMoodsAsync(Guid userId, DateTime from, DateTime to)
    {
        return await _context.MoodEntries
            .Where(m => m.UserId == userId && m.Timestamp >= from && m.Timestamp <= to)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<MoodEntry?> GetLatestMoodAsync(Guid userId)
    {
        return await _context.MoodEntries
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(MoodEntry moodEntry)
    {
        await _context.MoodEntries.AddAsync(moodEntry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MoodEntry moodEntry)
    {
        _context.MoodEntries.Update(moodEntry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var moodEntry = await GetByIdAsync(id);
        if (moodEntry != null)
        {
            _context.MoodEntries.Remove(moodEntry);
            await _context.SaveChangesAsync();
        }
    }
}
