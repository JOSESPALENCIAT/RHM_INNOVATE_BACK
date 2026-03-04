using RHM.Domain.Entities;

namespace RHM.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetActiveByTenantAsync(Guid tenantId);
    Task<IEnumerable<Subscription>> GetByTenantAsync(Guid tenantId);
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<Subscription> UpdateAsync(Subscription subscription);
}
