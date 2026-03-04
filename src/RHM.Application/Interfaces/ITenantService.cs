using RHM.Application.DTOs.Tenants;

namespace RHM.Application.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetAllAsync();
    Task<TenantDto?> GetByIdAsync(Guid id);
    Task<TenantDto> UpdateAsync(Guid id, TenantDto dto);
    Task SetActiveAsync(Guid id, bool isActive);
}
