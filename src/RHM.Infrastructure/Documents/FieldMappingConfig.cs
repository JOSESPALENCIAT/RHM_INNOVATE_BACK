using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RHM.Infrastructure.Documents;

/// <summary>
/// Configuración de mapeo entre campos dinámicos de un formulario
/// y variables clínicas sys_* del Motor de Estratificación.
/// Una por formulario (upsert). Almacenada en MongoDB.
/// </summary>
public class FieldMappingConfig
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string FormId   { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Clave = nombre del campo en el formulario (ej. "peso_corporal_kg").
    /// Valor = variable clínica sys_* destino (ej. "sys_imc").
    /// </summary>
    public Dictionary<string, string> Mappings { get; set; } = new();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
