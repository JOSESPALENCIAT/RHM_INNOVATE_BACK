using RHM.Application.DTOs.Risk;

namespace RHM.Infrastructure.Services.Algorithms;

/// <summary>
/// Patient Health Questionnaire-9 (PHQ-9) — Escala validada de depresión.
/// Fuente: Kroenke et al., Journal of General Internal Medicine 2001.
/// Cada ítem: 0=Nunca, 1=Varios días, 2=Más de la mitad de los días, 3=Casi todos los días.
/// Rango total: 0–27 puntos.
/// </summary>
public static class Phq9Algorithm
{
    public static AlgorithmScoreDto Calculate(ConsolidatedPatientProfile p)
    {
        if (p.Phq9Items is null || p.Phq9Items.Length < 9)
        {
            return new AlgorithmScoreDto
            {
                Algorithm = "PHQ-9", Score = 0, Category = "InsuficienteDatos",
                Confidence = p.Phq9Items is null ? 0 : Math.Round((double)p.Phq9Items.Length / 9, 2),
                Inputs = new() { ["items_disponibles"] = (p.Phq9Items?.Length ?? 0).ToString() }
            };
        }

        // Clamp valores al rango válido 0-3
        var items = p.Phq9Items.Select(i => Math.Clamp(i, 0, 3)).ToArray();
        int total = items.Sum();

        // Ítem 9 (ideación suicida): cualquier puntuación > 0 es bandera roja
        bool ideacionSuicida = items[8] > 0;

        var category = total switch
        {
            <= 4  => "Minimo",
            <= 9  => "Leve",
            <= 14 => "Moderado",
            <= 19 => "ModeradamenteGrave",
            _     => "Grave"
        };

        var inputs = new Dictionary<string, string>();
        for (int i = 0; i < items.Length; i++)
            inputs[$"phq9_item_{i + 1}"] = items[i].ToString();
        inputs["total"] = total.ToString();
        if (ideacionSuicida) inputs["alerta_ideacion_suicida"] = "true";

        return new AlgorithmScoreDto
        {
            Algorithm  = "PHQ-9",
            Score      = total,
            Category   = category,
            Confidence = 1.0,
            Inputs     = inputs
        };
    }
}
