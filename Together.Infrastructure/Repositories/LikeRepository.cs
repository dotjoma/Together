using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly TogetherDbContext _context;

    public LikeRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task AddAsync(Like like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Like like)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid postId, Guid userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }
}
