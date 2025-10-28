using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class ConnectionRequestRepository : IConnectionRequestRepository
{
    private readonly TogetherDbContext _context;

    public ConnectionRequestRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<ConnectionRequest?> GetByIdAsync(Guid id)
    {
        return await _context.ConnectionRequests
            .Include(cr => cr.FromUser)
            .Include(cr => cr.ToUser)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<IEnumerable<ConnectionRequest>> GetPendingRequestsForUserAsync(Guid userId)
    {
        return await _context.ConnectionRequests
            .Include(cr => cr.FromUser)
            .Include(cr => cr.ToUser)
            .Where(cr => cr.ToUserId == userId && cr.Status == ConnectionRequestStatus.Pending)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    public async Task<ConnectionRequest?> GetPendingRequestBetweenUsersAsync(Guid fromUserId, Guid toUserId)
    {
        return await _context.ConnectionRequests
            .FirstOrDefaultAsync(cr => 
                cr.FromUserId == fromUserId && 
                cr.ToUserId == toUserId && 
                cr.Status == ConnectionRequestStatus.Pending);
    }

    public async Task AddAsync(ConnectionRequest request)
    {
        await _context.ConnectionRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ConnectionRequest request)
    {
        _context.ConnectionRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = await _context.ConnectionRequests.FindAsync(id);
        if (request != null)
        {
            _context.ConnectionRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}
