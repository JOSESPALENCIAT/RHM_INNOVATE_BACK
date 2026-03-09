using RHM.Application.DTOs.Risk;
using RHM.Infrastructure.Services.Algorithms;

namespace RHM.Tests.Algorithms;

public class OncologicalRedFlagsTests
{
    private static readonly DateTime Today = DateTime.UtcNow;

    // Helper: perfil femenino ≥40 con todos los tamizajes al día
    private static ConsolidatedPatientProfile FemaleUpToDate(int age = 45) => new()
    {
        Age = age,
        Sex = "Female",
        UltimaCitologia    = Today.AddYears(-1),   // hace 1 año → vigente (≤3)
        UltimaMamografia   = Today.AddYears(-1),   // hace 1 año → vigente (≤2)
        UltimaColonoscopia = age >= 50 ? Today.AddYears(-2) : null,
        AntecedentesCancerFamiliar = false
    };

    private static ConsolidatedPatientProfile MaleProfile(int age = 55) => new()
    {
        Age = age,
        Sex = "Male",
        AntecedentesCancerFamiliar = false
    };

    // ── Categorías ──────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_NoFlags_ReturnsBajo()
    {
        var result = OncologicalRedFlags.Evaluate(FemaleUpToDate(age: 45));
        Assert.Equal("Bajo", result.Category);
        Assert.Empty(result.RedFlags);
    }

    [Fact]
    public void Evaluate_OneFlag_ReturnsModerated()
    {
        // Citología vencida (>3 años), mamografía al día
        var p = new ConsolidatedPatientProfile
        {
            Age = 45, Sex = "Female",
            UltimaCitologia  = Today.AddYears(-4),  // vencida
            UltimaMamografia = Today.AddYears(-1),  // vigente
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Equal("Moderado", result.Category);
        Assert.Single(result.RedFlags);
    }

    [Fact]
    public void Evaluate_TwoFlags_ReturnsAlto()
    {
        // Citología vencida + mamografía vencida
        var p = new ConsolidatedPatientProfile
        {
            Age = 45, Sex = "Female",
            UltimaCitologia  = Today.AddYears(-4),  // vencida (>3 años)
            UltimaMamografia = Today.AddYears(-3),  // vencida (>2 años)
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Equal("Alto", result.Category);
        Assert.Equal(2, result.RedFlags.Count);
    }

    [Fact]
    public void Evaluate_ThreeOrMoreFlags_ReturnsMuyAlto()
    {
        // Citología + mamografía + colonoscopia vencidas
        var p = new ConsolidatedPatientProfile
        {
            Age = 55, Sex = "Female",
            UltimaCitologia    = Today.AddYears(-5),  // vencida
            UltimaMamografia   = Today.AddYears(-4),  // vencida
            UltimaColonoscopia = Today.AddYears(-12), // vencida
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Equal("MuyAlto", result.Category);
        Assert.True(result.RedFlags.Count >= 3);
    }

    // ── Reglas de tamizaje individual ───────────────────────────────────

    [Fact]
    public void Evaluate_CitologiaSinRegistro_AddsFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 30, Sex = "Female",
            UltimaCitologia = null,
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Contains(result.RedFlags, f => f.Contains("Citología"));
    }

    [Fact]
    public void Evaluate_CitologiaVigente_NoFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 30, Sex = "Female",
            UltimaCitologia = Today.AddYears(-2), // ≤3 años → vigente
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("Citología"));
    }

    [Fact]
    public void Evaluate_CitologiaNotApplicableForMale_NoFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 40, Sex = "Male",
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("Citología"));
    }

    [Fact]
    public void Evaluate_MamografiaSinRegistroAge40Plus_AddsFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 42, Sex = "Female",
            UltimaCitologia  = Today.AddYears(-1),
            UltimaMamografia = null,
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Contains(result.RedFlags, f => f.Contains("Mamografía"));
    }

    [Fact]
    public void Evaluate_MamografiaNotApplicableBeforeAge40_NoFlag()
    {
        // Mujer 35 años → mamografía no aplica aún
        var p = new ConsolidatedPatientProfile
        {
            Age = 35, Sex = "Female",
            UltimaCitologia  = Today.AddYears(-1),
            UltimaMamografia = null,
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("Mamografía"));
    }

    [Fact]
    public void Evaluate_MamografiaVencida_AddsFlag()
    {
        // Última mamografía hace 3 años → >2 → vencida
        var p = new ConsolidatedPatientProfile
        {
            Age = 50, Sex = "Female",
            UltimaCitologia  = Today.AddYears(-1),
            UltimaMamografia = Today.AddYears(-3),
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Contains(result.RedFlags, f => f.Contains("Mamografía"));
    }

    [Fact]
    public void Evaluate_ColonoscopyAtAge50Plus_SinRegistro_AddsFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 52, Sex = "Male",
            UltimaColonoscopia = null,
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Contains(result.RedFlags, f => f.Contains("Colonoscopia"));
    }

    [Fact]
    public void Evaluate_ColonoscopyNotApplicableBeforeAge50_NoFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 48, Sex = "Male",
            UltimaColonoscopia = null,
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("Colonoscopia"));
    }

    [Fact]
    public void Evaluate_ColonoscopyVencida_AddsFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 55, Sex = "Male",
            UltimaColonoscopia = Today.AddYears(-12),
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Contains(result.RedFlags, f => f.Contains("Colonoscopia"));
    }

    [Fact]
    public void Evaluate_ColonoscopyVigente_NoFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 55, Sex = "Male",
            UltimaColonoscopia = Today.AddYears(-5),
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("Colonoscopia"));
    }

    // ── Antecedentes familiares ─────────────────────────────────────────

    [Fact]
    public void Evaluate_AntecedentesCancerFamiliar_AddsFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 40, Sex = "Male",
            AntecedentesCancerFamiliar = true
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.Contains(result.RedFlags, f => f.Contains("familiar"));
    }

    [Fact]
    public void Evaluate_SinAntecedentesFamiliares_NoFamiliarFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 40, Sex = "Male",
            AntecedentesCancerFamiliar = false
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("familiar"));
    }

    [Fact]
    public void Evaluate_FamiliarNull_NoFamiliarFlag()
    {
        var p = new ConsolidatedPatientProfile
        {
            Age = 40, Sex = "Male",
            AntecedentesCancerFamiliar = null
        };
        var result = OncologicalRedFlags.Evaluate(p);
        Assert.DoesNotContain(result.RedFlags, f => f.Contains("familiar"));
    }

    // ── Confianza ──────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ConfidenceIsNonNegative()
    {
        var result = OncologicalRedFlags.Evaluate(FemaleUpToDate());
        Assert.True(result.Confidence >= 0);
    }

    [Fact]
    public void Evaluate_FullFemale50_ConfidenceAbove0()
    {
        // Female 50: citología(1) + mamografía(1) + colonoscopia(1) + familiar(1) = 4 vars evaluated
        var result = OncologicalRedFlags.Evaluate(FemaleUpToDate(age: 50));
        Assert.True(result.Confidence > 0);
    }
}
