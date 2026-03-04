using RHM.Application.DTOs.Forms;

namespace RHM.Application.Interfaces;

public interface IFormResponseService
{
    Task<IEnumerable<FormResponseDto>> GetByFormAsync(string formId);
    Task<FormResponseDto> SubmitAsync(string formId, string tenantId, SubmitFormDto dto, string? ipAddress);
}
