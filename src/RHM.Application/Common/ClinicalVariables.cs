namespace RHM.Application.Common;

/// <summary>
/// Variables clínicas sys_* reconocidas por el Motor de Estratificación de Riesgo.
/// Estas son las únicas claves válidas como destino de un FieldMapping.
/// </summary>
public static class ClinicalVariables
{
    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        // Antropometría
        "sys_imc",
        "sys_perimetro_abdominal",

        // Cardiovascular / Metabólico
        "sys_tabaquismo",
        "sys_diabetes",
        "sys_hta",
        "sys_pas",
        "sys_tto_hta",

        // Hábitos
        "sys_actividad_fisica",
        "sys_dieta_frutas",

        // Historia clínica
        "sys_glucosa_alta",
        "sys_diabetes_familiar_1",
        "sys_diabetes_familiar_2",
        "sys_ca_familiar",

        // Tamizaje oncológico
        "sys_ultima_citologia",
        "sys_ultima_mamografia",
        "sys_ultima_colonoscopia",

        // PHQ-9 (ítems individuales)
        "sys_phq9_1", "sys_phq9_2", "sys_phq9_3", "sys_phq9_4", "sys_phq9_5",
        "sys_phq9_6", "sys_phq9_7", "sys_phq9_8", "sys_phq9_9",

        // GAD-7 (ítems individuales)
        "sys_gad7_1", "sys_gad7_2", "sys_gad7_3", "sys_gad7_4",
        "sys_gad7_5", "sys_gad7_6", "sys_gad7_7",
    };

    /// <summary>Etiqueta legible para mostrar en el UI.</summary>
    public static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sys_imc"]                  = "IMC (kg/m²)",
        ["sys_perimetro_abdominal"]  = "Perímetro abdominal (cm)",
        ["sys_tabaquismo"]           = "Tabaquismo (Sí/No)",
        ["sys_diabetes"]             = "Diabetes diagnosticada (Sí/No)",
        ["sys_hta"]                  = "Hipertensión arterial (Sí/No)",
        ["sys_pas"]                  = "Presión arterial sistólica (mmHg)",
        ["sys_tto_hta"]              = "Tratamiento antihipertensivo (Sí/No)",
        ["sys_actividad_fisica"]     = "Actividad física ≥30 min/día (Sí/No)",
        ["sys_dieta_frutas"]         = "Consumo diario frutas/verduras (Sí/No)",
        ["sys_glucosa_alta"]         = "Historia de glucosa alta (Sí/No)",
        ["sys_diabetes_familiar_1"]  = "Antecedente familiar diabetes 1° grado (Sí/No)",
        ["sys_diabetes_familiar_2"]  = "Antecedente familiar diabetes 2° grado (Sí/No)",
        ["sys_ca_familiar"]          = "Antecedente familiar cáncer (Sí/No)",
        ["sys_ultima_citologia"]     = "Fecha última citología cervical",
        ["sys_ultima_mamografia"]    = "Fecha última mamografía",
        ["sys_ultima_colonoscopia"]  = "Fecha última colonoscopia",
        ["sys_phq9_1"]  = "PHQ-9 ítem 1 — Poco interés o placer",
        ["sys_phq9_2"]  = "PHQ-9 ítem 2 — Sentirse triste o sin esperanza",
        ["sys_phq9_3"]  = "PHQ-9 ítem 3 — Dificultad para dormir",
        ["sys_phq9_4"]  = "PHQ-9 ítem 4 — Cansancio o poca energía",
        ["sys_phq9_5"]  = "PHQ-9 ítem 5 — Poco apetito o comer en exceso",
        ["sys_phq9_6"]  = "PHQ-9 ítem 6 — Sentirse mal consigo mismo",
        ["sys_phq9_7"]  = "PHQ-9 ítem 7 — Dificultad de concentración",
        ["sys_phq9_8"]  = "PHQ-9 ítem 8 — Lentitud o agitación motora",
        ["sys_phq9_9"]  = "PHQ-9 ítem 9 — Pensamientos de muerte o hacerse daño",
        ["sys_gad7_1"]  = "GAD-7 ítem 1 — Nervioso o ansioso",
        ["sys_gad7_2"]  = "GAD-7 ítem 2 — Sin poder dejar de preocuparse",
        ["sys_gad7_3"]  = "GAD-7 ítem 3 — Preocupación excesiva",
        ["sys_gad7_4"]  = "GAD-7 ítem 4 — Dificultad para relajarse",
        ["sys_gad7_5"]  = "GAD-7 ítem 5 — Inquieto o intranquilo",
        ["sys_gad7_6"]  = "GAD-7 ítem 6 — Irritable o molesto fácilmente",
        ["sys_gad7_7"]  = "GAD-7 ítem 7 — Sensación de peligro o miedo",
    };
}
