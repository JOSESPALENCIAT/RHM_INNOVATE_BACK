using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RHM.Infrastructure.Documents;

public class FormResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string FormId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// UUID del paciente en Azure SQL (Patient.Id).
    /// Pivot que vincula este evento con el Master Patient Index.
    /// Se asigna en el pipeline post-submit por MasterPatientIndexService.
    /// </summary>
    public string? PatientId { get; set; }

    public Dictionary<string, string> Data { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? IpAddress { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Estado del pipeline de normalización n8n.</summary>
    public N8nPipelineStatus N8nStatus { get; set; } = new();
}

public class N8nPipelineStatus
{
    /// <summary>El municipio fue normalizado a código DIVIPOLA.</summary>
    public bool Normalized { get; set; } = false;

    /// <summary>El perfil de riesgo fue calculado.</summary>
    public bool RiskCalculated { get; set; } = false;

    /// <summary>La narrativa IA fue generada.</summary>
    public bool AiNarrativeGenerated { get; set; } = false;

    public DateTime? NormalizedAt { get; set; }
    public DateTime? RiskCalculatedAt { get; set; }
}
