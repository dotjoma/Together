using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface IVirtualPetRepository
{
    Task<VirtualPet?> GetByIdAsync(Guid id);
    Task<VirtualPet?> GetByConnectionIdAsync(Guid connectionId);
    Task AddAsync(VirtualPet pet);
    Task UpdateAsync(VirtualPet pet);
    Task DeleteAsync(Guid id);
}
