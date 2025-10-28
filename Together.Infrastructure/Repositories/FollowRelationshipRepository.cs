using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class FollowRelationshipRepository : IFollowRelationshipRepository
{
    private readonly TogetherDbContext _context;

    public FollowRelationshipRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<FollowRelationship?> GetByIdAsync(Guid id)
    {
        return await _context.FollowRelationships
            .Include(fr => fr.Follower)
            .Include(fr => fr.Following)
            .FirstOrDefaultAsync(fr => fr.Id == id);
    }

    public async Task<FollowRelationship?> GetByFollowerAndFollowingAsync(Guid followerId, Guid followingId)
    {
        return await _context.FollowRelationships
            .Include(fr => fr.Follower)
            .Include(fr => fr.Following)
            .FirstOrDefaultAsync(fr => fr.FollowerId == followerId && fr.FollowingId == followingId);
    }

    public async Task<IEnumerable<FollowRelationship>> GetFollowersAsync(Guid userId)
    {
        return await _context.FollowRelationships
            .Include(fr => fr.Follower)
            .Include(fr => fr.Following)
            .Where(fr => fr.FollowingId == userId && fr.Status == "accepted")
            .ToListAsync();
    }

    public async Task<IEnumerable<FollowRelationship>> GetFollowingAsync(Guid userId)
    {
        return await _context.FollowRelationships
            .Include(fr => fr.Follower)
            .Include(fr => fr.Following)
            .Where(fr => fr.FollowerId == userId && fr.Status == "accepted")
            .ToListAsync();
    }

    public async Task<IEnumerable<FollowRelationship>> GetPendingRequestsAsync(Guid userId)
    {
        return await _context.FollowRelationships
            .Include(fr => fr.Follower)
            .Include(fr => fr.Following)
            .Where(fr => fr.FollowingId == userId && fr.Status == "pending")
            .ToListAsync();
    }

    public async Task AddAsync(FollowRelationship followRelationship)
    {
        await _context.FollowRelationships.AddAsync(followRelationship);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FollowRelationship followRelationship)
    {
        _context.FollowRelationships.Update(followRelationship);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var followRelationship = await _context.FollowRelationships.FindAsync(id);
        if (followRelationship != null)
        {
            _context.FollowRelationships.Remove(followRelationship);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetFollowerCountAsync(Guid userId)
    {
        return await _context.FollowRelationships
            .CountAsync(fr => fr.FollowingId == userId && fr.Status == "accepted");
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _context.FollowRelationships
            .CountAsync(fr => fr.FollowerId == userId && fr.Status == "accepted");
    }
}
