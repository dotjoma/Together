using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly TogetherDbContext _context;

    public PostRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetUserPostsAsync(Guid userId, int skip, int take)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetFeedPostsAsync(Guid userId, int skip, int take)
    {
        // Get users that the current user is following
        var followingIds = await _context.FollowRelationships
            .Where(f => f.FollowerId == userId && f.Status == "accepted")
            .Select(f => f.FollowingId)
            .ToListAsync();

        // Get posts from followed users
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Where(p => followingIds.Contains(p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task AddAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}
