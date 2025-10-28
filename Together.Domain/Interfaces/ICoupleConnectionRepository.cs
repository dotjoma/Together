using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface ICoupleConnectionRepository
{
    Task<CoupleConnection?> GetByUserIdAsync(Guid userId);
    Task<CoupleConnection?> GetByIdAsync(Guid id);
    Task AddAsync(CoupleConnection connection);
    Task UpdateAsync(CoupleConnection connection);
    Task DeleteAsync(Guid id);
}
