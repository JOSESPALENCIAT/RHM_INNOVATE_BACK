namespace RHM.Application.DTOs.Patients;

/// <summary>
/// Grupo de posibles pacientes duplicados detectados en el MPI.
/// Los duplicados se detectan por DocNumber normalizado (sin espacios/guiones)
/// con mismo o distinto DocType dentro del mismo tenant.
/// </summary>
public class DuplicateGroupDto
{
    /// <summary>Clave de deduplicación usada para agrupar (DocNumber normalizado).</summary>
    public string NormalizedKey { get; set; } = string.Empty;

    /// <summary>Pacientes identificados como posibles duplicados.</summary>
    public List<PatientDto> Patients { get; set; } = [];
}

/// <summary>
/// Solicitud de fusión de dos registros de paciente.
/// Todos los datos clínicos (FormResponses y PatientRiskProfiles) del secundario
/// se reasignan al primario; el secundario se elimina de Azure SQL.
/// </summary>
public class MergePatientDto
{
    /// <summary>PatientId que se conserva (el "ganador").</summary>
    public Guid PrimaryId { get; set; }

    /// <summary>PatientId que se elimina después de reasignar sus datos.</summary>
    public Guid SecondaryId { get; set; }
}

/// <summary>Resultado de la operación de merge.</summary>
public class MergeResultDto
{
    public string PrimaryId { get; set; } = string.Empty;
    public int FormResponsesMigrated { get; set; }
    public int RiskProfilesMigrated { get; set; }
    public bool SecondaryDeleted { get; set; }
}
