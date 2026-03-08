using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.Interfaces;
using RHM.Shared.Constants;

namespace RHM.API.Controllers;

/// <summary>
/// Módulo de Estratificación de Riesgo.
/// Requiere rol AccountAdmin u Operator (datos propios del tenant).
/// </summary>
[ApiController]
[Route("api/risk")]
[Authorize]
public class RiskProfilesController : ControllerBase
{
    private readonly IRiskEngineService _riskEngine;
    private readonly INarrativeService _narrative;

    public RiskProfilesController(IRiskEngineService riskEngine, INarrativeService narrative)
    {
        _riskEngine = riskEngine;
        _narrative  = narrative;
    }

    private string TenantId =>
        User.FindFirstValue(RhmConstants.Claims.TenantId) ?? string.Empty;

    // ------------------------------------------------------------------ //
    //  CÁLCULO                                                            //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Calcula el perfil de riesgo para el paciente asociado a un submission.
    /// Disparado automáticamente por n8n post-normalización, o manualmente.
    /// </summary>
    [HttpPost("calculate/{submissionId}")]
    [Authorize(Roles = "AccountAdmin,Operator,SuperAdmin")]
    public async Task<IActionResult> Calculate(string submissionId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(TenantId)) return Unauthorized();
        try
        {
            var result = await _riskEngine.CalculateAsync(TenantId, submissionId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Recálculo manual del perfil de riesgo de un paciente (usando todos sus submissions).
    /// </summary>
    [HttpPost("recalculate/{patientId}")]
    [Authorize(Roles = "AccountAdmin")]
    public async Task<IActionResult> Recalculate(string patientId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(TenantId)) return Unauthorized();
        try
        {
            var result = await _riskEngine.RecalculateForPatientAsync(TenantId, patientId, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ------------------------------------------------------------------ //
    //  CONSULTAS                                                          //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Historial de perfiles de riesgo de un paciente (últimos 12 cálculos).
    /// </summary>
    [HttpGet("patient/{patientId}/history")]
    [Authorize(Roles = "AccountAdmin,Operator")]
    public async Task<IActionResult> GetHistory(string patientId, [FromQuery] int limit = 12)
    {
        if (string.IsNullOrEmpty(TenantId)) return Unauthorized();
        var history = await _riskEngine.GetHistoryAsync(TenantId, patientId, limit);
        return Ok(history);
    }

    /// <summary>
    /// Dashboard poblacional: lista paginada de pacientes con su riesgo compuesto más reciente.
    /// Filtros: category (Bajo|Moderado|Alto|MuyAlto), page, pageSize.
    /// </summary>
    // ------------------------------------------------------------------ //
    //  NARRATIVA IA                                                       //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Genera una narrativa clínica con IA (Claude) para el perfil de riesgo
    /// más reciente del paciente. Persiste el texto en MongoDB.
    /// </summary>
    [HttpPost("narrative/{patientId}")]
    [Authorize(Roles = "AccountAdmin,Operator")]
    public async Task<IActionResult> GenerateNarrative(string patientId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(TenantId)) return Unauthorized();
        try
        {
            var result = await _narrative.GenerateAsync(TenantId, patientId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = "Error comunicando con Claude API.", detail = ex.Message });
        }
    }

    [HttpGet("population")]
    [Authorize(Roles = "AccountAdmin,Operator")]
    public async Task<IActionResult> GetPopulation(
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (string.IsNullOrEmpty(TenantId)) return Unauthorized();
        var summary = await _riskEngine.GetPopulationSummaryAsync(TenantId, category, page, pageSize);
        return Ok(summary);
    }
}
