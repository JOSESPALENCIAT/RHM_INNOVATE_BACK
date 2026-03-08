namespace RHM.Application.DTOs.FieldMapping;

/// <summary>
/// Payload que n8n envía de vuelta tras procesar un submission:
/// normalización DIVIPOLA + campos clínicos mapeados por LLM.
/// </summary>
public class N8nCallbackDto
{
    /// <summary>ID del tenant (incluido por n8n desde el webhook original).</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>ID del FormResponse en MongoDB.</summary>
    public string SubmissionId { get; set; } = string.Empty;

    /// <summary>Código DIVIPOLA de 5 dígitos resuelto por el LLM (ej. "05001").</summary>
    public string? DivipolaMunCode { get; set; }

    /// <summary>Código de departamento DANE de 2 dígitos (ej. "05").</summary>
    public string? DivipolaDeptCode { get; set; }

    /// <summary>
    /// Campos normalizados con clave sys_* para inyectar en FormResponse.Data.
    /// El Risk Engine los tomará en el siguiente cálculo.
    /// </summary>
    public Dictionary<string, string>? NormalizedFields { get; set; }
}
