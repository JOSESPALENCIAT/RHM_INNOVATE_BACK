using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.FieldMapping;
using RHM.Application.Interfaces;

namespace RHM.API.Controllers;

/// <summary>
/// Recibe callbacks de n8n tras la normalización DIVIPOLA y el mapeo de variables clínicas por LLM.
/// Este endpoint NO requiere autenticación Bearer (n8n usa una clave secreta en header).
/// </summary>
[ApiController]
[Route("api/n8n")]
public class N8nController : ControllerBase
{
    private readonly IFieldMappingService _fieldMapping;
    private readonly IRiskEngineService _riskEngine;
    private readonly IConfiguration _config;

    public N8nController(
        IFieldMappingService fieldMapping,
        IRiskEngineService riskEngine,
        IConfiguration config)
    {
        _fieldMapping = fieldMapping;
        _riskEngine   = riskEngine;
        _config       = config;
    }

    /// <summary>
    /// n8n llama este endpoint después de normalizar el submission:
    /// 1. Resuelve municipio → código DIVIPOLA (LLM).
    /// 2. Mapea campos del formulario a variables clínicas sys_*.
    /// 3. Activa el Risk Engine para recalcular el perfil del paciente.
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> Callback(
        [FromHeader(Name = "X-N8n-Secret")] string? secret,
        [FromBody] N8nCallbackDto dto)
    {
        // Verificar secreto compartido (configurado en appsettings)
        var expected = _config["N8n:CallbackSecret"];
        if (!string.IsNullOrWhiteSpace(expected) && secret != expected)
            return Unauthorized(new { error = "X-N8n-Secret inválido." });

        if (string.IsNullOrWhiteSpace(dto.SubmissionId))
            return BadRequest(new { error = "submissionId requerido." });

        // TenantId puede venir en el body o en header (n8n lo incluye del webhook original)
        var tenantId = dto.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { error = "tenantId requerido." });

        // 1. Procesar normalización + inyección de campos clínicos
        var patientId = await _fieldMapping.ProcessN8nCallbackAsync(tenantId, dto);

        // 2. Fire-and-forget: recalcular riesgo con los datos normalizados
        if (patientId is not null)
            _ = Task.Run(() => _riskEngine.RecalculateForPatientAsync(
                tenantId, patientId, CancellationToken.None));

        return Ok(new { message = "Callback procesado.", patientId });
    }
}
