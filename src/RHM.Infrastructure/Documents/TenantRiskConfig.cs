using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RHM.Infrastructure.Documents;

/// <summary>
/// Configuración parametrizable del Motor de Estratificación por tenant.
/// Almacenada en MongoDB; una entrada por tenant (upsert).
/// Permite habilitar/deshabilitar algoritmos y ajustar umbrales.
/// </summary>
public class TenantRiskConfig
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    // ------------------------------------------------------------------ //
    //  Algoritmos habilitados                                             //
    // ------------------------------------------------------------------ //

    /// <summary>Framingham — riesgo cardiovascular a 10 años.</summary>
    public bool EnableFramingham { get; set; } = true;

    /// <summary>FINDRISC — riesgo de diabetes tipo 2.</summary>
    public bool EnableFindrisc { get; set; } = true;

    /// <summary>PHQ-9 — tamizaje de depresión.</summary>
    public bool EnablePhq9 { get; set; } = true;

    /// <summary>GAD-7 — tamizaje de ansiedad.</summary>
    public bool EnableGad7 { get; set; } = true;

    /// <summary>Banderas rojas oncológicas (tamizaje de cáncer).</summary>
    public bool EnableOncological { get; set; } = true;

    // ------------------------------------------------------------------ //
    //  Umbrales de confianza                                             //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Confianza mínima para incluir un algoritmo en el riesgo compuesto.
    /// Si un algoritmo tiene Confidence menor a este valor retorna InsuficienteDatos.
    /// Rango: 0.0 – 1.0. Por defecto 0.5 (50 % de variables requeridas disponibles).
    /// </summary>
    public double MinConfidenceThreshold { get; set; } = 0.5;

    /// <summary>
    /// Umbral de alerta de datos incompletos para el dashboard.
    /// Si la confianza promedio del perfil es menor a este valor,
    /// se muestra una alerta de "datos insuficientes para estratificación".
    /// Rango: 0.0 – 1.0. Por defecto 0.7.
    /// </summary>
    public double DataCompletenessAlertThreshold { get; set; } = 0.7;

    // ------------------------------------------------------------------ //
    //  Pesos del riesgo compuesto                                        //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Pesos de cada dimensión en el riesgo compuesto (deben sumar 1.0).
    /// Si es null, se usa la lógica por defecto (máximo entre dimensiones válidas).
    /// </summary>
    public CompositeWeights? Weights { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Pesos de las dimensiones del riesgo compuesto.</summary>
public class CompositeWeights
{
    public double Cardiovascular { get; set; } = 0.30;
    public double Metabolic      { get; set; } = 0.25;
    public double Mental         { get; set; } = 0.25;
    public double Oncological    { get; set; } = 0.20;
}
