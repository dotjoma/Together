using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;

namespace Together.Infrastructure.Repositories;

public class VirtualPetRepository : IVirtualPetRepository
{
    private readonly TogetherDbContext _context;

    public VirtualPetRepository(TogetherDbContext context)
    {
        _context = context;
    }

    public async Task<VirtualPet?> GetByIdAsync(Guid id)
    {
        return await _context.VirtualPets
            .Include(p => p.Connection)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<VirtualPet?> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _context.VirtualPets
            .Include(p => p.Connection)
            .FirstOrDefaultAsync(p => p.ConnectionId == connectionId);
    }

    public async Task AddAsync(VirtualPet pet)
    {
        await _context.VirtualPets.AddAsync(pet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VirtualPet pet)
    {
        _context.VirtualPets.Update(pet);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var pet = await _context.VirtualPets.FindAsync(id);
        if (pet != null)
        {
            _context.VirtualPets.Remove(pet);
            await _context.SaveChangesAsync();
        }
    }
}
