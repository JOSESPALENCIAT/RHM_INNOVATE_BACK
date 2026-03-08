using RHM.Application.DTOs.Risk;

namespace RHM.Application.Interfaces;

/// <summary>
/// Motor de Estratificación de Riesgo.
/// Consolida el historial del paciente y calcula los scores clínicos validados.
/// </summary>
public interface IRiskEngineService
{
    /// <summary>
    /// Calcula el perfil de riesgo disparado por un submission específico.
    /// Consolida todos los form_responses del paciente en ese tenant.
    /// </summary>
    Task<PatientRiskProfileDto> CalculateAsync(string tenantId, string submissionId, CancellationToken ct = default);

    /// <summary>
    /// Recálculo manual o batch usando el patientId directamente.
    /// </summary>
    Task<PatientRiskProfileDto> RecalculateForPatientAsync(string tenantId, string patientId, CancellationToken ct = default);

    /// <summary>
    /// Historial de perfiles de riesgo de un paciente (orden descendente).
    /// </summary>
    Task<IEnumerable<PatientRiskProfileDto>> GetHistoryAsync(string tenantId, string patientId, int limit = 12);

    /// <summary>
    /// Listado paginado de pacientes con su riesgo más reciente para el dashboard poblacional.
    /// </summary>
    Task<IEnumerable<PatientRiskSummaryDto>> GetPopulationSummaryAsync(
        string tenantId, string? categoryFilter = null, int page = 1, int pageSize = 50);
}
