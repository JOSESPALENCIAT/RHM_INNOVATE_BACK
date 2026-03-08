namespace RHM.Application.DTOs.Patients;

/// <summary>
/// Resultado de búsqueda en el catálogo DIVIPOLA.
/// Usado en el autocomplete del formulario público.
/// </summary>
public class DivipolaDto
{
    public string MunCode { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Display => $"{Municipio} — {Departamento}";
}
