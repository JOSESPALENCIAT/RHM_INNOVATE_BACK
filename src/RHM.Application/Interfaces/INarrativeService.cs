using RHM.Application.DTOs.Risk;

namespace RHM.Application.Interfaces;

public interface INarrativeService
{
    /// <summary>
    /// Genera una narrativa clínica en lenguaje natural para el perfil de riesgo
    /// más reciente del paciente, usando Claude API. Persiste el resultado en MongoDB.
    /// </summary>
    Task<NarrativeResponseDto> GenerateAsync(string tenantId, string patientId, CancellationToken ct = default);
}
