using RHM.Application.DTOs.Risk;

namespace RHM.Application.Interfaces;

public interface ITenantRiskConfigService
{
    /// <summary>
    /// Obtiene la configuración del motor para un tenant.
    /// Si no existe, retorna los valores por defecto (sin persistir).
    /// </summary>
    Task<TenantRiskConfigDto> GetAsync(string tenantId);

    /// <summary>Guarda (upsert) la configuración del motor para un tenant.</summary>
    Task<TenantRiskConfigDto> SaveAsync(string tenantId, SaveTenantRiskConfigDto dto);
}
