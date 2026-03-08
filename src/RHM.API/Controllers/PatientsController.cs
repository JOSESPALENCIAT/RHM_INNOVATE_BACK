using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.Patients;
using RHM.Application.Interfaces;

namespace RHM.API.Controllers;

/// <summary>
/// Gestión del Master Patient Index (MPI): deduplicación y merge de registros.
/// Acceso exclusivo del SuperAdmin.
/// </summary>
[ApiController]
[Route("api/patients")]
[Authorize(Roles = "SuperAdmin")]
public class PatientsController : ControllerBase
{
    private readonly IMasterPatientIndexService _mpi;

    public PatientsController(IMasterPatientIndexService mpi) => _mpi = mpi;

    /// <summary>
    /// Detecta grupos de pacientes con DocNumber normalizado idéntico.
    /// Solo retorna grupos con 2 o más registros (posibles duplicados).
    /// </summary>
    [HttpGet("duplicates")]
    public async Task<IActionResult> GetDuplicates([FromQuery] string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { error = "El parámetro tenantId es obligatorio." });
        var groups = await _mpi.GetDuplicatesAsync(tenantId);
        return Ok(groups);
    }

    /// <summary>
    /// Fusiona dos registros de paciente dentro del tenant indicado.
    /// Todos los FormResponses y PatientRiskProfiles del secundario se reasignan
    /// al primario; el secundario se elimina de Azure SQL.
    /// </summary>
    [HttpPost("merge")]
    public async Task<IActionResult> Merge([FromQuery] string tenantId, [FromBody] MergePatientDto dto)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest(new { error = "El parámetro tenantId es obligatorio." });
        if (dto.PrimaryId == dto.SecondaryId)
            return BadRequest(new { error = "El paciente primario y secundario no pueden ser el mismo." });

        try
        {
            var result = await _mpi.MergeAsync(tenantId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
