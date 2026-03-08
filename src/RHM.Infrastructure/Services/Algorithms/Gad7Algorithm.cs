using RHM.Application.DTOs.Risk;

namespace RHM.Infrastructure.Services.Algorithms;

/// <summary>
/// Generalized Anxiety Disorder Scale-7 (GAD-7) — Escala validada de ansiedad.
/// Fuente: Spitzer et al., Archives of Internal Medicine 2006.
/// Cada ítem: 0=Nunca, 1=Varios días, 2=Más de la mitad, 3=Casi todos los días.
/// Rango total: 0–21 puntos.
/// </summary>
public static class Gad7Algorithm
{
    public static AlgorithmScoreDto Calculate(ConsolidatedPatientProfile p)
    {
        if (p.Gad7Items is null || p.Gad7Items.Length < 7)
        {
            return new AlgorithmScoreDto
            {
                Algorithm = "GAD-7", Score = 0, Category = "InsuficienteDatos",
                Confidence = p.Gad7Items is null ? 0 : Math.Round((double)p.Gad7Items.Length / 7, 2),
                Inputs = new() { ["items_disponibles"] = (p.Gad7Items?.Length ?? 0).ToString() }
            };
        }

        var items = p.Gad7Items.Select(i => Math.Clamp(i, 0, 3)).ToArray();
        int total = items.Sum();

        var category = total switch
        {
            <= 4  => "Minimo",
            <= 9  => "Leve",
            <= 14 => "Moderado",
            _     => "Grave"
        };

        var inputs = new Dictionary<string, string>();
        for (int i = 0; i < items.Length; i++)
            inputs[$"gad7_item_{i + 1}"] = items[i].ToString();
        inputs["total"] = total.ToString();

        return new AlgorithmScoreDto
        {
            Algorithm  = "GAD-7",
            Score      = total,
            Category   = category,
            Confidence = 1.0,
            Inputs     = inputs
        };
    }
}
