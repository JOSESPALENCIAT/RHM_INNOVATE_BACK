using RHM.Application.DTOs.Patients;

namespace RHM.Application.Interfaces;

/// <summary>
/// Master Patient Index (MPI) — Resuelve la identidad única del paciente en Azure SQL.
/// Implementado en Infrastructure para acceder a AppDbContext.
/// </summary>
public interface IMasterPatientIndexService
{
    /// <summary>
    /// Busca o crea el paciente en Azure SQL usando TipoDoc + NumDoc como llave natural.
    /// Devuelve el UUID interno del paciente (Patient.Id) para vincularlo en MongoDB.
    /// </summary>
    Task<string> ResolvePatientKeyAsync(string tenantId, ResolvePatientDto dto);

    /// <summary>
    /// Actualiza el código DIVIPOLA del municipio en el registro del paciente.
    /// Llamado por el pipeline n8n post-normalización.
    /// </summary>
    Task UpdateDivipolaAsync(string patientId, string munCode, string deptCode);

    /// <summary>
    /// Detecta grupos de pacientes con DocNumber normalizado idéntico dentro del mismo tenant.
    /// Solo retorna grupos con más de un registro (posibles duplicados).
    /// </summary>
    Task<IEnumerable<DuplicateGroupDto>> GetDuplicatesAsync(string tenantId);

    /// <summary>
    /// Fusiona dos registros de paciente: reasigna FormResponses y PatientRiskProfiles
    /// del secundario al primario, luego elimina el secundario de Azure SQL.
    /// </summary>
    Task<MergeResultDto> MergeAsync(string tenantId, MergePatientDto dto);
}
