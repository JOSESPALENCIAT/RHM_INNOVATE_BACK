using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RHM.Infrastructure.Documents;

/// <summary>
/// Perfil de riesgo calculado por el Motor de Estratificación.
/// Almacenado en MongoDB. El campo PatientId es la FK al registro en Azure SQL (Patient.Id).
/// Se crea una nueva entrada cada vez que se recalcula (historial longitudinal).
/// </summary>
public class PatientRiskProfile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>UUID del paciente en Azure SQL — pivot MPI.</summary>
    public string PatientId { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Id del FormResponse que disparó este cálculo.</summary>
    public string? TriggeredBySubmissionId { get; set; }

    /// <summary>Resumen de los datos disponibles al momento del cálculo.</summary>
    public DataSourceSummary DataSource { get; set; } = new();

    public RiskScores Scores { get; set; } = new();

    public CompositeRisk CompositeRisk { get; set; } = new();

    /// <summary>Narrativa clínica generada por IA (Claude). Vacía hasta Sprint 6.</summary>
    public string AiNarrative { get; set; } = string.Empty;
}

public class DataSourceSummary
{
    public int SubmissionsConsidered { get; set; }
    public DateTime? OldestDataPoint { get; set; }

    /// <summary>Variables requeridas por los algoritmos que no estaban disponibles.</summary>
    public List<string> MissingCriticalVars { get; set; } = [];
}

public class RiskScores
{
    public AlgorithmScore? Cardiovascular { get; set; }
    public AlgorithmScore? Metabolic { get; set; }
    public MentalHealthScore? Mental { get; set; }
    public OncologicalScore? Oncological { get; set; }
}

public class AlgorithmScore
{
    /// <summary>Nombre del algoritmo usado (ej. "Framingham", "FINDRISC").</summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>Puntaje numérico calculado.</summary>
    public double Score { get; set; }

    /// <summary>Categoría resultante: Bajo, Moderado, Alto, MuyAlto, InsuficienteDatos.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Porcentaje de variables requeridas que estaban disponibles (0.0–1.0).</summary>
    public double Confidence { get; set; }

    /// <summary>Snapshot de los valores de variables usadas en el cálculo.</summary>
    public Dictionary<string, object> Inputs { get; set; } = new();
}

public class MentalHealthScore
{
    public AlgorithmScore? Phq9 { get; set; }
    public AlgorithmScore? Gad7 { get; set; }
}

public class OncologicalScore
{
    public List<string> RedFlags { get; set; } = [];
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class CompositeRisk
{
    /// <summary>Categoría de riesgo global: Bajo, Moderado, Alto, MuyAlto.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Dimensión de riesgo dominante (la de mayor categoría).</summary>
    public string Dominant { get; set; } = string.Empty;
}
