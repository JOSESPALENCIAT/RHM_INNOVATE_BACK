namespace RHM.Application.DTOs.Patients;

/// <summary>
/// Datos de identidad mínimos extraídos del bloque demográfico del formulario
/// para resolver o crear el paciente en el MPI (Azure SQL).
/// Corresponde a los campos sys_* del Form Builder.
/// </summary>
public class ResolvePatientDto
{
    public string DocType { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string BiologicalSex { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Municipio { get; set; }
}
