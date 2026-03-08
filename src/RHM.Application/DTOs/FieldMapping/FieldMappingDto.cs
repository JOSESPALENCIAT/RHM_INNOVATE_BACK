namespace RHM.Application.DTOs.FieldMapping;

public class FieldMappingDto
{
    public string FormId { get; set; } = string.Empty;

    /// <summary>Clave = campo del formulario, valor = sys_* variable clínica.</summary>
    public Dictionary<string, string> Mappings { get; set; } = new();

    public DateTime? UpdatedAt { get; set; }
}

public class SaveFieldMappingDto
{
    /// <summary>Clave = campo del formulario, valor = sys_* variable clínica.</summary>
    public Dictionary<string, string> Mappings { get; set; } = new();
}
