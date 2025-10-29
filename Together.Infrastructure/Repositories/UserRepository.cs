using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TogetherDbContext _context;

    public UserRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Value == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<User>();
        }

        var normalizedQuery = query.Trim().ToLower();

        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Username.ToLower().Contains(normalizedQuery) || 
                       u.Email.Value.ToLower().Contains(normalizedQuery))
            .Where(u => u.Visibility == ProfileVisibility.Public)
            .OrderBy(u => u.Username)
            .Take(limit)
            .ToListAsync();
    }
}
