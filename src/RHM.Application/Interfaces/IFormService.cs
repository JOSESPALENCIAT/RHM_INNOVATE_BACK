using RHM.Application.DTOs.Forms;

namespace RHM.Application.Interfaces;

public interface IFormService
{
    Task<IEnumerable<FormSchemaDto>> GetByTenantAsync(string tenantId);
    Task<FormSchemaDto?> GetByIdAsync(string id);
    Task<FormSchemaDto?> GetByPublicUrlAsync(string publicUrl);
    Task<FormSchemaDto> CreateAsync(FormSchemaDto dto);
    Task<FormSchemaDto> UpdateAsync(string id, FormSchemaDto dto);
    Task<FormSchemaDto> PublishAsync(string id);
    Task DeleteAsync(string id);
}
