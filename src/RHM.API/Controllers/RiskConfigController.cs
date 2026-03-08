using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.Risk;
using RHM.Application.Interfaces;

namespace RHM.API.Controllers;

/// <summary>
/// Gestiona la configuración parametrizable del Motor de Estratificación de Riesgo por tenant.
/// Acceso exclusivo del SuperAdmin.
/// </summary>
[ApiController]
[Route("api/risk/config")]
[Authorize(Roles = "SuperAdmin")]
public class RiskConfigController : ControllerBase
{
    private readonly ITenantRiskConfigService _service;

    public RiskConfigController(ITenantRiskConfigService service) => _service = service;

    /// <summary>Obtiene la configuración actual del motor para el tenant indicado.</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { error = "El parámetro tenantId es obligatorio." });
        var config = await _service.GetAsync(tenantId);
        return Ok(config);
    }

    /// <summary>Guarda (upsert) la configuración del motor para el tenant indicado.</summary>
    [HttpPut]
    public async Task<IActionResult> Save([FromQuery] string tenantId, [FromBody] SaveTenantRiskConfigDto dto)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { error = "El parámetro tenantId es obligatorio." });
        try
        {
            var result = await _service.SaveAsync(tenantId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Restablece la configuración a los valores por defecto del sistema para el tenant indicado.</summary>
    [HttpDelete]
    public async Task<IActionResult> Reset([FromQuery] string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { error = "El parámetro tenantId es obligatorio." });
        var defaults = new SaveTenantRiskConfigDto();
        var result = await _service.SaveAsync(tenantId, defaults);
        return Ok(result);
    }
}
