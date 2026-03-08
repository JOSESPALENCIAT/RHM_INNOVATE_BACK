using RHM.Application.DTOs.Risk;

namespace RHM.Infrastructure.Services.Algorithms;

/// <summary>
/// Banderas Rojas Oncológicas basadas en guías de tamizaje colombianas (MinSalud / INC).
/// No es un modelo predictivo de probabilidad; detecta brechas en detección temprana.
/// </summary>
public static class OncologicalRedFlags
{
    private const int ConfidenceVarsCount = 5; // vars que se evalúan para confianza

    public static OncologicalScoreDto Evaluate(ConsolidatedPatientProfile p)
    {
        var flags = new List<string>();
        var today = DateTime.UtcNow;
        int available = 0;

        // --- Cáncer de Cuello Uterino ---
        // GuíA MinSalud: citología cada 3 años en mujeres ≥ 25 años
        if (p.Sex == "Female" && p.Age >= 25)
        {
            available++;
            if (p.UltimaCitologia.HasValue)
            {
                var yearsAgo = (today - p.UltimaCitologia.Value).TotalDays / 365.25;
                if (yearsAgo > 3)
                    flags.Add($"Citología cervical: última hace {yearsAgo:F0} años (guía: cada 3 años)");
            }
            else
            {
                flags.Add("Citología cervical: sin registro — tamizaje pendiente");
            }
        }

        // --- Cáncer de Mama ---
        // MinSalud: mamografía cada 2 años en mujeres ≥ 40 años
        if (p.Sex == "Female" && p.Age >= 40)
        {
            available++;
            if (p.UltimaMamografia.HasValue)
            {
                var yearsAgo = (today - p.UltimaMamografia.Value).TotalDays / 365.25;
                if (yearsAgo > 2)
                    flags.Add($"Mamografía: última hace {yearsAgo:F0} años (guía: cada 2 años)");
            }
            else
            {
                flags.Add("Mamografía: sin registro — tamizaje pendiente");
            }
        }

        // --- Cáncer Colorrectal ---
        // USPSTF / MinSalud: colonoscopia cada 10 años en ≥ 50 años
        if (p.Age >= 50)
        {
            available++;
            if (p.UltimaColonoscopia.HasValue)
            {
                var yearsAgo = (today - p.UltimaColonoscopia.Value).TotalDays / 365.25;
                if (yearsAgo > 10)
                    flags.Add($"Colonoscopia: última hace {yearsAgo:F0} años (guía: cada 10 años)");
            }
            else
            {
                flags.Add("Colonoscopia: sin registro — tamizaje pendiente");
            }
        }

        // --- Antecedentes familiares ---
        if (p.AntecedentesCancerFamiliar.HasValue)
        {
            available++;
            if (p.AntecedentesCancerFamiliar == true)
                flags.Add("Antecedente oncológico familiar de primer grado");
        }

        var confidence = ConfidenceVarsCount > 0
            ? Math.Round((double)available / ConfidenceVarsCount, 2)
            : 0;

        var category = flags.Count switch
        {
            0            => "Bajo",
            1            => "Moderado",
            2            => "Alto",
            _            => "MuyAlto"
        };

        return new OncologicalScoreDto
        {
            RedFlags   = flags,
            Category   = category,
            Confidence = confidence
        };
    }
}
