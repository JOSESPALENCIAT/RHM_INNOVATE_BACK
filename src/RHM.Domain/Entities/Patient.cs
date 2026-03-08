using RHM.Domain.Enums;

namespace RHM.Domain.Entities;

/// <summary>
/// Master Patient Index (MPI) — Identidad única del paciente en Azure SQL.
/// Una fila por paciente. La llave natural es (TenantId, DocType, DocNumber).
/// El campo Id (UUID) es la FK que vincula todos los documentos en MongoDB.
/// </summary>
public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Tenant al que pertenece este registro de paciente.</summary>
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // --- Identidad (llave natural Colombia: TipoDoc + NumDoc) ---
    public DocumentType DocType { get; set; }
    public string DocNumber { get; set; } = string.Empty;

    // --- Datos demográficos mínimos obligatorios ---
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public BiologicalSex BiologicalSex { get; set; }

    // --- Datos de contacto y localización (actualizables) ---
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    /// <summary>Código DIVIPOLA del municipio de residencia (5 dígitos). Normalizado por n8n.</summary>
    public string? DivipolaMunCode { get; set; }

    /// <summary>Código DIVIPOLA del departamento (2 dígitos). Derivado del municipio.</summary>
    public string? DivipolaDeptCode { get; set; }

    // --- Auditoría ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // --- Ayudante calculado (no persistido) ---
    public int Age => DateTime.UtcNow.Year - BirthDate.Year -
                      (DateTime.UtcNow.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
}
