using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class CoupleConnectionRepository : ICoupleConnectionRepository
{
    private readonly TogetherDbContext _context;

    public CoupleConnectionRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<CoupleConnection?> GetByUserIdAsync(Guid userId)
    {
        return await _context.CoupleConnections
            .Include(cc => cc.User1)
            .Include(cc => cc.User2)
            .Include(cc => cc.VirtualPet)
            .FirstOrDefaultAsync(cc => 
                (cc.User1Id == userId || cc.User2Id == userId) && 
                cc.Status == ConnectionStatus.Active);
    }

    public async Task<CoupleConnection?> GetByIdAsync(Guid id)
    {
        return await _context.CoupleConnections
            .Include(cc => cc.User1)
            .Include(cc => cc.User2)
            .Include(cc => cc.VirtualPet)
            .FirstOrDefaultAsync(cc => cc.Id == id);
    }

    public async Task AddAsync(CoupleConnection connection)
    {
        await _context.CoupleConnections.AddAsync(connection);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CoupleConnection connection)
    {
        _context.CoupleConnections.Update(connection);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var connection = await _context.CoupleConnections.FindAsync(id);
        if (connection != null)
        {
            _context.CoupleConnections.Remove(connection);
            await _context.SaveChangesAsync();
        }
    }
}
