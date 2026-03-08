using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.Common;
using RHM.Application.DTOs.FieldMapping;
using RHM.Application.Interfaces;

namespace RHM.API.Controllers;

/// <summary>
/// Gestiona el mapeo entre campos dinámicos de un formulario
/// y las variables clínicas sys_* del Motor de Estratificación.
/// Acceso restringido a AccountAdmin y SuperAdmin.
/// </summary>
[ApiController]
[Route("api/forms/{formId}/field-mappings")]
[Authorize(Roles = "AccountAdmin,SuperAdmin")]
public class FieldMappingController : ControllerBase
{
    private readonly IFieldMappingService _service;

    public FieldMappingController(IFieldMappingService service)
        => _service = service;

    private string TenantId =>
        User.FindFirst("TenantId")?.Value ?? string.Empty;

    // GET api/forms/{formId}/field-mappings
    [HttpGet]
    public async Task<IActionResult> Get(string formId)
    {
        var dto = await _service.GetByFormIdAsync(TenantId, formId);
        return dto is null ? NotFound() : Ok(dto);
    }

    // PUT api/forms/{formId}/field-mappings
    [HttpPut]
    public async Task<IActionResult> Save(string formId, [FromBody] SaveFieldMappingDto dto)
    {
        try
        {
            var result = await _service.SaveAsync(TenantId, formId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET api/forms/{formId}/field-mappings/catalog
    /// <summary>Devuelve el catálogo de variables clínicas disponibles.</summary>
    [HttpGet("catalog")]
    public IActionResult GetCatalog()
    {
        var catalog = ClinicalVariables.Labels
            .Select(kvp => new { key = kvp.Key, label = kvp.Value })
            .OrderBy(x => x.key);

        return Ok(catalog);
    }
}
