namespace RHM.Application.DTOs.Risk;

/// <summary>
/// Perfil consolidado del paciente construido desde dos fuentes:
/// - Azure SQL (Patients): datos demográficos confiables (edad, sexo, etc.)
/// - MongoDB (form_responses): variables clínicas longitudinales (sys_* extraídas de formularios)
/// Es la entrada del Motor de Riesgo. Se construye en memoria, no se persiste.
/// </summary>
public class ConsolidatedPatientProfile
{
    // --- Identidad (desde Azure SQL) ---
    public string PatientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;      // "Male" | "Female" | "Indeterminate"
    public string? DivipolaMunCode { get; set; }

    // --- Variables clínicas agregadas (desde MongoDB form_responses) ---
    // Consolidadas del formulario más reciente que contenga cada variable.

    // Cardiovascular / Metabólico
    public double? Imc { get; set; }                    // sys_imc (kg/m²)
    public double? PerimetroAbdominal { get; set; }     // sys_perimetro_abdominal (cm)
    public bool? Tabaquismo { get; set; }               // sys_tabaquismo
    public bool? DiabetesDiagnostico { get; set; }      // sys_diabetes
    public bool? HipertensionDiagnostico { get; set; }  // sys_hta
    public double? PresionArterialSistolica { get; set; }// sys_pas
    public bool? TratamientoHipertension { get; set; }  // sys_tto_hta
    public bool? ActividadFisicaDiaria { get; set; }    // sys_actividad_fisica (≥30 min/día)
    public bool? ConsumeFrutasVerduras { get; set; }    // sys_dieta_frutas
    public bool? GlucosaAltaHistoria { get; set; }      // sys_glucosa_alta
    public bool? DiabetesFamiliar1Grado { get; set; }   // sys_diabetes_familiar_1
    public bool? DiabetesFamiliar2Grado { get; set; }   // sys_diabetes_familiar_2

    // Salud mental (PHQ-9 / GAD-7 — cada ítem 0-3)
    public int[]? Phq9Items { get; set; }               // sys_phq9_1 ... sys_phq9_9
    public int[]? Gad7Items { get; set; }               // sys_gad7_1 ... sys_gad7_7

    // Oncológico
    public bool? AntecedentesCancerFamiliar { get; set; }// sys_ca_familiar
    public DateTime? UltimaCitologia { get; set; }      // sys_ultima_citologia
    public DateTime? UltimaMamografia { get; set; }     // sys_ultima_mamografia
    public DateTime? UltimaColonoscopia { get; set; }   // sys_ultima_colonoscopia

    // --- Metadatos de consolidación ---
    public int SubmissionsConsidered { get; set; }
    public DateTime? OldestDataPoint { get; set; }
}
