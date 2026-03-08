namespace RHM.Domain.Enums;

/// <summary>
/// Tipos de documento de identificación válidos en Colombia (estándar RETHUS / MinSalud).
/// </summary>
public enum DocumentType
{
    CC  = 1,  // Cédula de Ciudadanía
    TI  = 2,  // Tarjeta de Identidad
    CE  = 3,  // Cédula de Extranjería
    PA  = 4,  // Pasaporte
    RC  = 5,  // Registro Civil
    MS  = 6,  // Menor sin identificación
    NIT = 7,  // NIT (personas jurídicas, uso excepcional)
    AS  = 8,  // Adulto sin identificación
    CN  = 9   // Certificado de Nacido Vivo
}
