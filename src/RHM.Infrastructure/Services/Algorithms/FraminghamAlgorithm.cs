using RHM.Application.DTOs.Risk;

namespace RHM.Infrastructure.Services.Algorithms;

/// <summary>
/// Algoritmo de Framingham simplificado para estimar riesgo cardiovascular a 10 años.
/// Fuente: Wilson et al., Circulation 1998 — versión adaptada con IMC como proxy de lípidos.
/// Validez clínica: herramienta de apoyo, no reemplaza evaluación médica.
/// </summary>
public static class FraminghamAlgorithm
{
    // Variables requeridas para confianza completa
    private static readonly string[] RequiredVars =
        ["Age", "Sex", "Tabaquismo", "HipertensionDiagnostico", "DiabetesDiagnostico", "Imc"];

    public static AlgorithmScoreDto Calculate(ConsolidatedPatientProfile p)
    {
        var available = CountAvailable(p);
        var confidence = (double)available / RequiredVars.Length;

        if (confidence < 0.5)
            return Insufficient("Framingham", confidence, p);

        // Puntuación por sexo (tablas de Framingham simplificadas con IMC)
        double points = p.Sex == "Male"
            ? CalculateMale(p)
            : CalculateFemale(p);

        // Convertir puntos a porcentaje de riesgo a 10 años
        double risk10y = PointsToRisk(points, p.Sex);

        var inputs = BuildInputs(p);
        var category = risk10y switch
        {
            < 10 => "Bajo",
            < 20 => "Moderado",
            < 30 => "Alto",
            _    => "MuyAlto"
        };

        return new AlgorithmScoreDto
        {
            Algorithm  = "Framingham",
            Score      = Math.Round(risk10y, 1),
            Category   = category,
            Confidence = Math.Round(confidence, 2),
            Inputs     = inputs
        };
    }

    private static double CalculateMale(ConsolidatedPatientProfile p)
    {
        double pts = 0;

        // Edad
        pts += p.Age switch
        {
            < 35 => -9,
            < 40 => -4,
            < 45 => 0,
            < 50 => 3,
            < 55 => 6,
            < 60 => 8,
            < 65 => 10,
            < 70 => 11,
            _    => 12
        };

        // IMC como proxy (en lugar de colesterol)
        if (p.Imc.HasValue)
        {
            pts += p.Imc.Value switch
            {
                < 25  => -3,
                < 30  => 0,
                _     => 2
            };
        }

        // Fumador
        if (p.Tabaquismo == true) pts += 4;

        // Diabetes
        if (p.DiabetesDiagnostico == true) pts += 3;

        // Hipertensión
        if (p.HipertensionDiagnostico == true)
            pts += p.TratamientoHipertension == true ? 5 : 3;

        // PAS (si disponible)
        if (p.PresionArterialSistolica.HasValue)
        {
            pts += p.PresionArterialSistolica.Value switch
            {
                < 120 => -3,
                < 130 => 0,
                < 140 => 1,
                < 160 => 2,
                _     => 3
            };
        }

        return pts;
    }

    private static double CalculateFemale(ConsolidatedPatientProfile p)
    {
        double pts = 0;

        // Edad
        pts += p.Age switch
        {
            < 35 => -7,
            < 40 => -3,
            < 45 => 0,
            < 50 => 3,
            < 55 => 6,
            < 60 => 8,
            < 65 => 10,
            < 70 => 12,
            _    => 14
        };

        // IMC
        if (p.Imc.HasValue)
        {
            pts += p.Imc.Value switch
            {
                < 25  => -2,
                < 30  => 0,
                _     => 2
            };
        }

        // Fumadora
        if (p.Tabaquismo == true) pts += 3;

        // Diabetes
        if (p.DiabetesDiagnostico == true) pts += 4;

        // Hipertensión
        if (p.HipertensionDiagnostico == true)
            pts += p.TratamientoHipertension == true ? 5 : 3;

        // PAS
        if (p.PresionArterialSistolica.HasValue)
        {
            pts += p.PresionArterialSistolica.Value switch
            {
                < 120 => -3,
                < 130 => 0,
                < 140 => 1,
                < 160 => 2,
                _     => 3
            };
        }

        return pts;
    }

    /// <summary>
    /// Convierte puntos Framingham a % de riesgo a 10 años (tablas OMS simplificadas).
    /// </summary>
    private static double PointsToRisk(double points, string sex)
    {
        // Tabla masculina
        if (sex == "Male")
        {
            return points switch
            {
                <= -3  => 1,
                <= 0   => 2,
                <= 3   => 4,
                <= 5   => 6,
                <= 7   => 9,
                <= 9   => 13,
                <= 11  => 18,
                <= 13  => 24,
                <= 15  => 30,
                _      => 36
            };
        }
        // Tabla femenina
        return points switch
        {
            <= -2  => 1,
            <= 1   => 2,
            <= 4   => 3,
            <= 6   => 5,
            <= 8   => 7,
            <= 10  => 10,
            <= 12  => 14,
            <= 14  => 19,
            <= 16  => 24,
            _      => 30
        };
    }

    private static int CountAvailable(ConsolidatedPatientProfile p)
    {
        int n = 0;
        if (p.Age > 0) n++;
        if (!string.IsNullOrEmpty(p.Sex)) n++;
        if (p.Tabaquismo.HasValue) n++;
        if (p.HipertensionDiagnostico.HasValue) n++;
        if (p.DiabetesDiagnostico.HasValue) n++;
        if (p.Imc.HasValue) n++;
        return n;
    }

    private static Dictionary<string, string> BuildInputs(ConsolidatedPatientProfile p) => new()
    {
        ["edad"]           = p.Age.ToString(),
        ["sexo"]           = p.Sex,
        ["imc"]            = p.Imc?.ToString("F1") ?? "N/D",
        ["tabaquismo"]     = p.Tabaquismo?.ToString() ?? "N/D",
        ["diabetes"]       = p.DiabetesDiagnostico?.ToString() ?? "N/D",
        ["hipertension"]   = p.HipertensionDiagnostico?.ToString() ?? "N/D",
        ["tto_hta"]        = p.TratamientoHipertension?.ToString() ?? "N/D",
        ["pas"]            = p.PresionArterialSistolica?.ToString("F0") ?? "N/D"
    };

    private static AlgorithmScoreDto Insufficient(string algorithm, double confidence, ConsolidatedPatientProfile p) =>
        new() { Algorithm = algorithm, Score = 0, Category = "InsuficienteDatos",
                Confidence = Math.Round(confidence, 2), Inputs = BuildInputs(p) };
}
