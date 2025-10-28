using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class SharedEventRepository : ISharedEventRepository
{
    private readonly TogetherDbContext _context;

    public SharedEventRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<SharedEvent?> GetByIdAsync(Guid id)
    {
        return await _context.SharedEvents
            .Include(e => e.Creator)
            .Include(e => e.Connection)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<SharedEvent>> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _context.SharedEvents
            .Include(e => e.Creator)
            .Where(e => e.ConnectionId == connectionId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SharedEvent>> GetUpcomingEventsAsync(Guid connectionId, DateTime from, int limit)
    {
        return await _context.SharedEvents
            .Include(e => e.Creator)
            .Where(e => e.ConnectionId == connectionId && e.EventDate >= from)
            .OrderBy(e => e.EventDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<SharedEvent>> GetEventsByDateRangeAsync(Guid connectionId, DateTime from, DateTime to)
    {
        return await _context.SharedEvents
            .Include(e => e.Creator)
            .Where(e => e.ConnectionId == connectionId && e.EventDate >= from && e.EventDate <= to)
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }

    public async Task AddAsync(SharedEvent sharedEvent)
    {
        await _context.SharedEvents.AddAsync(sharedEvent);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SharedEvent sharedEvent)
    {
        _context.SharedEvents.Update(sharedEvent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var sharedEvent = await _context.SharedEvents.FindAsync(id);
        if (sharedEvent != null)
        {
            _context.SharedEvents.Remove(sharedEvent);
            await _context.SaveChangesAsync();
        }
    }
}
