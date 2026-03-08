namespace RHM.Application.DTOs.Risk;

/// <summary>
/// Resultado completo del motor de estratificación para un paciente.
/// </summary>
public class PatientRiskProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public string? TriggeredBySubmissionId { get; set; }

    public DataSourceSummaryDto DataSource { get; set; } = new();
    public RiskScoresDto Scores { get; set; } = new();
    public CompositeRiskDto CompositeRisk { get; set; } = new();
    public string AiNarrative { get; set; } = string.Empty;
}

public class DataSourceSummaryDto
{
    public int SubmissionsConsidered { get; set; }
    public DateTime? OldestDataPoint { get; set; }
    public List<string> MissingCriticalVars { get; set; } = [];
}

public class RiskScoresDto
{
    public AlgorithmScoreDto? Cardiovascular { get; set; }
    public AlgorithmScoreDto? Metabolic { get; set; }
    public MentalHealthScoreDto? Mental { get; set; }
    public OncologicalScoreDto? Oncological { get; set; }
}

public class AlgorithmScoreDto
{
    /// <summary>Nombre del algoritmo: "Framingham" | "FINDRISC"</summary>
    public string Algorithm { get; set; } = string.Empty;
    public double Score { get; set; }
    /// <summary>Bajo | Moderado | Alto | MuyAlto | InsuficienteDatos</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>0.0–1.0: porcentaje de variables disponibles sobre las requeridas.</summary>
    public double Confidence { get; set; }
    public Dictionary<string, string> Inputs { get; set; } = new();
}

public class MentalHealthScoreDto
{
    public AlgorithmScoreDto? Phq9 { get; set; }
    public AlgorithmScoreDto? Gad7 { get; set; }
}

public class OncologicalScoreDto
{
    public List<string> RedFlags { get; set; } = [];
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class CompositeRiskDto
{
    public string Category { get; set; } = string.Empty;
    public string Dominant { get; set; } = string.Empty;
}

/// <summary>Resumen de paciente para listados poblacionales.</summary>
public class PatientRiskSummaryDto
{
    public string PatientId { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string? Municipio { get; set; }
    public string CompositeCategory { get; set; } = string.Empty;
    public string DominantRisk { get; set; } = string.Empty;
    public DateTime? LastCalculatedAt { get; set; }
    public int TotalSubmissions { get; set; }
}
