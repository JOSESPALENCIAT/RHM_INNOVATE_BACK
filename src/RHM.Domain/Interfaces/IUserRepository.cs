using RHM.Domain.Entities;

namespace RHM.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId);
    Task<int> CountByTenantAsync(Guid tenantId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
