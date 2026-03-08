namespace RHM.Domain.Enums;

/// <summary>
/// Sexo biológico según clasificación clínica estándar.
/// Separado de género para uso en algoritmos de riesgo clínico (Framingham, FINDRISC).
/// </summary>
public enum BiologicalSex
{
    Male           = 1,
    Female         = 2,
    Indeterminate  = 3
}
