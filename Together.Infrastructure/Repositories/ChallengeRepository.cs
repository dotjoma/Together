using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class ChallengeRepository : IChallengeRepository
{
    private readonly TogetherDbContext _context;

    public ChallengeRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<Challenge> GetByIdAsync(Guid id)
    {
        var challenge = await _context.Challenges
            .Include(c => c.Connection)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null)
            throw new KeyNotFoundException($"Challenge with ID {id} not found");

        return challenge;
    }

    public async Task<IEnumerable<Challenge>> GetActiveChallengesAsync(Guid connectionId)
    {
        return await _context.Challenges
            .Where(c => c.ConnectionId == connectionId && c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Challenge>> GetExpiredChallengesAsync()
    {
        return await _context.Challenges
            .Where(c => c.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<Challenge?> GetTodaysChallengeAsync(Guid connectionId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.Challenges
            .Where(c => c.ConnectionId == connectionId 
                     && c.CreatedAt >= today 
                     && c.CreatedAt < tomorrow)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Challenge challenge)
    {
        await _context.Challenges.AddAsync(challenge);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Challenge challenge)
    {
        _context.Challenges.Update(challenge);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var challenge = await GetByIdAsync(id);
        _context.Challenges.Remove(challenge);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCoupleScoreAsync(Guid connectionId)
    {
        return await _context.Challenges
            .Where(c => c.ConnectionId == connectionId 
                     && c.CompletedByUser1 
                     && c.CompletedByUser2)
            .SumAsync(c => c.Points);
    }
}
