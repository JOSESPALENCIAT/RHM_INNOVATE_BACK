namespace RHM.Application.DTOs.Risk;

/// <summary>DTO de lectura de la configuración del Motor de Estratificación.</summary>
public class TenantRiskConfigDto
{
    public bool EnableFramingham  { get; set; } = true;
    public bool EnableFindrisc    { get; set; } = true;
    public bool EnablePhq9        { get; set; } = true;
    public bool EnableGad7        { get; set; } = true;
    public bool EnableOncological { get; set; } = true;

    public double MinConfidenceThreshold          { get; set; } = 0.5;
    public double DataCompletenessAlertThreshold  { get; set; } = 0.7;

    public CompositeWeightsDto? Weights { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>DTO de escritura (PUT) de la configuración del Motor de Estratificación.</summary>
public class SaveTenantRiskConfigDto
{
    public bool EnableFramingham  { get; set; } = true;
    public bool EnableFindrisc    { get; set; } = true;
    public bool EnablePhq9        { get; set; } = true;
    public bool EnableGad7        { get; set; } = true;
    public bool EnableOncological { get; set; } = true;

    public double MinConfidenceThreshold         { get; set; } = 0.5;
    public double DataCompletenessAlertThreshold { get; set; } = 0.7;

    /// <summary>
    /// Si es null, el compuesto se calcula por máximo entre dimensiones válidas.
    /// Si se provee, se calcula como promedio ponderado (deben sumar ≈ 1.0).
    /// </summary>
    public CompositeWeightsDto? Weights { get; set; }
}

public class CompositeWeightsDto
{
    public double Cardiovascular { get; set; } = 0.30;
    public double Metabolic      { get; set; } = 0.25;
    public double Mental         { get; set; } = 0.25;
    public double Oncological    { get; set; } = 0.20;
}
