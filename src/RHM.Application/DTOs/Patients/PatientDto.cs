namespace RHM.Application.DTOs.Patients;

/// <summary>
/// Datos del paciente devueltos en listados y consultas del MPI.
/// </summary>
public class PatientDto
{
    public Guid Id { get; set; }
    public string DocType { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public string BiologicalSex { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? DivipolaMunCode { get; set; }
    public string? DivipolaDeptCode { get; set; }
    public string? Municipio { get; set; }
    public string? Departamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
