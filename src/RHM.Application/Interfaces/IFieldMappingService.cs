using RHM.Application.DTOs.FieldMapping;

namespace RHM.Application.Interfaces;

public interface IFieldMappingService
{
    /// <summary>Obtiene la configuración de mapeo de un formulario.</summary>
    Task<FieldMappingDto?> GetByFormIdAsync(string tenantId, string formId);

    /// <summary>Guarda (upsert) el mapeo de campos de un formulario.</summary>
    Task<FieldMappingDto> SaveAsync(string tenantId, string formId, SaveFieldMappingDto dto);

    /// <summary>
    /// Aplica los mappings a un diccionario de datos de respuesta.
    /// Agrega entradas con clave sys_* a partir de los campos mapeados.
    /// No sobreescribe entradas sys_* ya existentes.
    /// </summary>
    Task<Dictionary<string, string>> ApplyMappingsAsync(string formId, Dictionary<string, string> data);

    /// <summary>
    /// Procesa el callback de n8n: inyecta campos normalizados en el FormResponse
    /// y actualiza el DIVIPOLA del paciente en Azure SQL si fue resuelto.
    /// Retorna el patientId asociado al submission (para retrigger del Risk Engine).
    /// </summary>
    Task<string?> ProcessN8nCallbackAsync(string tenantId, N8nCallbackDto dto);
}
