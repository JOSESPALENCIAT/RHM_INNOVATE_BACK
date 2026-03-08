using RHM.Application.DTOs.Risk;

namespace RHM.Infrastructure.Services.Algorithms;

/// <summary>
/// Finnish Diabetes Risk Score (FINDRISC) — OMS recomendado para Colombia.
/// Predice riesgo de desarrollar Diabetes Tipo 2 en los próximos 10 años.
/// Fuente: Lindström &amp; Tuomilehto, Diabetes Care 2003.
/// Rango: 0-26 puntos.
/// </summary>
public static class FindriscAlgorithm
{
    private static readonly string[] RequiredVars =
        ["Age", "Imc", "ActividadFisicaDiaria", "ConsumeFrutasVerduras",
         "HipertensionDiagnostico", "GlucosaAltaHistoria", "DiabetesFamiliar1Grado"];

    public static AlgorithmScoreDto Calculate(ConsolidatedPatientProfile p)
    {
        var available = CountAvailable(p);
        var confidence = (double)available / RequiredVars.Length;

        if (confidence < 0.5)
            return Insufficient(confidence, p);

        int score = 0;

        // 1. Edad
        score += p.Age switch
        {
            < 45 => 0,
            < 55 => 2,
            < 65 => 3,
            _    => 4
        };

        // 2. IMC (kg/m²)
        if (p.Imc.HasValue)
        {
            score += p.Imc.Value switch
            {
                < 25   => 0,
                < 30   => 1,
                _      => 3
            };
        }

        // 3. Perímetro abdominal (si disponible — según sexo)
        if (p.PerimetroAbdominal.HasValue)
        {
            bool isMale = p.Sex == "Male";
            score += isMale
                ? p.PerimetroAbdominal.Value switch { < 94 => 0, < 102 => 3, _ => 4 }
                : p.PerimetroAbdominal.Value switch { < 80 => 0, < 88  => 3, _ => 4 };
        }

        // 4. Actividad física ≥ 30 min/día
        if (p.ActividadFisicaDiaria == false) score += 2;

        // 5. Consumo diario de frutas y verduras
        if (p.ConsumeFrutasVerduras == false) score += 1;

        // 6. Historia de hipertensión arterial
        if (p.HipertensionDiagnostico == true) score += 2;

        // 7. Historia de glucosa alta (glucemia en ayunas alterada o pre-diabetes)
        if (p.GlucosaAltaHistoria == true) score += 5;

        // 8. Antecedentes familiares de diabetes
        if (p.DiabetesFamiliar1Grado == true)
            score += 5;
        else if (p.DiabetesFamiliar2Grado == true)
            score += 3;

        var category = score switch
        {
            <= 6  => "Bajo",
            <= 11 => "LigeramenteElevado",
            <= 14 => "Moderado",
            <= 20 => "Alto",
            _     => "MuyAlto"
        };

        return new AlgorithmScoreDto
        {
            Algorithm  = "FINDRISC",
            Score      = score,
            Category   = category,
            Confidence = Math.Round(confidence, 2),
            Inputs     = BuildInputs(p, score)
        };
    }

    private static int CountAvailable(ConsolidatedPatientProfile p)
    {
        int n = 0;
        if (p.Age > 0) n++;
        if (p.Imc.HasValue) n++;
        if (p.ActividadFisicaDiaria.HasValue) n++;
        if (p.ConsumeFrutasVerduras.HasValue) n++;
        if (p.HipertensionDiagnostico.HasValue) n++;
        if (p.GlucosaAltaHistoria.HasValue) n++;
        if (p.DiabetesFamiliar1Grado.HasValue || p.DiabetesFamiliar2Grado.HasValue) n++;
        return n;
    }

    private static Dictionary<string, string> BuildInputs(ConsolidatedPatientProfile p, int score) => new()
    {
        ["edad"]                    = p.Age.ToString(),
        ["imc"]                     = p.Imc?.ToString("F1") ?? "N/D",
        ["perimetro_abdominal"]     = p.PerimetroAbdominal?.ToString("F0") + " cm" ?? "N/D",
        ["actividad_fisica"]        = p.ActividadFisicaDiaria?.ToString() ?? "N/D",
        ["dieta_frutas"]            = p.ConsumeFrutasVerduras?.ToString() ?? "N/D",
        ["hta"]                     = p.HipertensionDiagnostico?.ToString() ?? "N/D",
        ["glucosa_alta"]            = p.GlucosaAltaHistoria?.ToString() ?? "N/D",
        ["diabetes_familiar_1"]     = p.DiabetesFamiliar1Grado?.ToString() ?? "N/D",
        ["diabetes_familiar_2"]     = p.DiabetesFamiliar2Grado?.ToString() ?? "N/D",
        ["puntuacion_total"]        = score.ToString()
    };

    private static AlgorithmScoreDto Insufficient(double confidence, ConsolidatedPatientProfile p) =>
        new() { Algorithm = "FINDRISC", Score = 0, Category = "InsuficienteDatos",
                Confidence = Math.Round(confidence, 2), Inputs = BuildInputs(p, 0) };
}
