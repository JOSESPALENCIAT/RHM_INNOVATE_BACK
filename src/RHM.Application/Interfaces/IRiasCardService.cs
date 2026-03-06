using RHM.Application.DTOs.Rias;

namespace RHM.Application.Interfaces;

public interface IRiasCardService
{
    /// <summary>Returns the global RIAS config, or null if none saved (frontend uses static defaults).</summary>
    Task<TenantRiasConfigDto?> GetAsync();

    /// <summary>Upserts the global RIAS card configuration (singleton document, shared by all tenants).</summary>
    Task<TenantRiasConfigDto> SaveAsync(string userId, TenantRiasConfigDto dto);

    /// <summary>Deletes the global config so all tenants revert to static defaults.</summary>
    Task DeleteAsync();
}
