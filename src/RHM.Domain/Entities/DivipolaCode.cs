namespace RHM.Domain.Entities;

/// <summary>
/// Catálogo oficial DIVIPOLA (DANE) — División político-administrativa de Colombia.
/// Usado como lookup local cuando la normalización por IA no está disponible.
/// Código municipio: 5 dígitos (2 dept + 3 mun). Código dept: 2 dígitos.
/// </summary>
public class DivipolaCode
{
    /// <summary>Código municipio DIVIPOLA de 5 dígitos. Es la PK.</summary>
    public string MunCode { get; set; } = string.Empty;

    /// <summary>Código departamento DIVIPOLA de 2 dígitos.</summary>
    public string DeptCode { get; set; } = string.Empty;

    public string Departamento { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;

    /// <summary>Nombre normalizado para búsqueda (sin tildes, mayúsculas).</summary>
    public string MunicipioNormalized { get; set; } = string.Empty;
}
