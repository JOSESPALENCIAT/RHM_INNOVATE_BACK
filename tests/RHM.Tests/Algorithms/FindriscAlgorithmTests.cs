using RHM.Application.DTOs.Risk;
using RHM.Infrastructure.Services.Algorithms;

namespace RHM.Tests.Algorithms;

public class FindriscAlgorithmTests
{
    // Helper: perfil mínimo con todas las variables requeridas → confianza 1.0
    private static ConsolidatedPatientProfile FullProfile(
        int age = 40,
        double imc = 24,
        bool actividadFisica = true,
        bool frutas = true,
        bool hta = false,
        bool glucosaAlta = false,
        bool? familiar1 = false,
        bool? familiar2 = false,
        string sex = "Male",
        double? perimetro = null) => new()
    {
        Age = age,
        Sex = sex,
        Imc = imc,
        ActividadFisicaDiaria = actividadFisica,
        ConsumeFrutasVerduras = frutas,
        HipertensionDiagnostico = hta,
        GlucosaAltaHistoria = glucosaAlta,
        DiabetesFamiliar1Grado = familiar1,
        DiabetesFamiliar2Grado = familiar2,
        PerimetroAbdominal = perimetro
    };

    // ── Categorías ─────────────────────────────────────────────────────

    [Theory]
    // score=0: age<45(0) + imc<25(0) + activo(0) + frutas(0) + sinHTA(0) + sinGlucosa(0) + sinFamiliar(0)
    [InlineData(40, 24.0, true, true, false, false, false, false, "Bajo")]
    // score=6: age<55(2) + imc<30(1) + inactivo(2) + sinFrutas(1) → 6 → Bajo
    [InlineData(50, 27.0, false, false, false, false, false, false, "Bajo")]
    // score=7: age<55(2) + imc<25(0) + inactivo(2) + sinFrutas(1) + hta(2) → 7 → LigeramenteElevado
    [InlineData(50, 24.0, false, false, true, false, false, false, "LigeramenteElevado")]
    // score=11: age<65(3) + imc<30(1) + inactivo(2) + sinFrutas(1) + hta(2) + sinGlucosa(0) + familiar2(3) → 12? Let me recalculate
    // age<65=3, imc<30=1, inactivo=2, sinFrutas=1, hta=2, familiar2=3 → 12 → Moderado
    [InlineData(60, 27.0, false, false, true, false, false, true, "Moderado")]
    // score=15: age>=65(4) + imc>=30(3) + inactivo(2) + sinFrutas(1) + hta(2) + sinGlucosa(0) + sinFamiliar(0) → 12 → Moderado
    // Need more: + glucosaAlta(5) → 17 → Alto
    [InlineData(65, 31.0, false, false, false, true, false, false, "Alto")]
    // score=21: age>=65(4) + imc>=30(3) + inactivo(2) + sinFrutas(1) + hta(2) + glucosaAlta(5) + familiar1(5) → 22 → MuyAlto
    [InlineData(65, 31.0, false, false, true, true, true, false, "MuyAlto")]
    public void Calculate_ReturnsCorrectCategory(
        int age, double imc, bool activo, bool frutas, bool hta, bool glucosa,
        bool familiar1, bool familiar2, string expectedCategory)
    {
        var p = FullProfile(age, imc, activo, frutas, hta, glucosa, familiar1, familiar2);
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal(expectedCategory, result.Category);
    }

    // ── Boundaries exactas ──────────────────────────────────────────────

    [Fact]
    public void Calculate_Score6_IsBajo()
    {
        // age<45(0) + imc<25(0) + inactivo(2) + sinFrutas(1) + hta(2) + sinGlucosa(0) + sinFamiliar(0) = 5?
        // age<55(2) + imc<25(0) + inactivo(2) + sinFrutas(1) + noHta(0) = 5 → Bajo
        // Need exactly 6: age<55(2) + imc<30(1) + inactivo(2) + sinFrutas(1) = 6 → Bajo
        var p = FullProfile(age: 50, imc: 27, actividadFisica: false, frutas: false, hta: false, glucosaAlta: false);
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal(6, (int)result.Score);
        Assert.Equal("Bajo", result.Category);
    }

    [Fact]
    public void Calculate_Score7_IsLigeramenteElevado()
    {
        // age<55(2) + imc<25(0) + inactivo(2) + sinFrutas(1) + hta(2) = 7
        var p = FullProfile(age: 50, imc: 24, actividadFisica: false, frutas: false, hta: true, glucosaAlta: false);
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal(7, (int)result.Score);
        Assert.Equal("LigeramenteElevado", result.Category);
    }

    [Fact]
    public void Calculate_Score11_IsLigeramenteElevado()
    {
        // age<55(2) + imc<30(1) + inactivo(2) + sinFrutas(1) + hta(2) + familiar2(3) = 11 → LigeramenteElevado
        var p = FullProfile(age: 50, imc: 27, actividadFisica: false, frutas: false, hta: true, glucosaAlta: false, familiar1: false, familiar2: true);
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal(11, (int)result.Score);
        Assert.Equal("LigeramenteElevado", result.Category);
    }

    [Fact]
    public void Calculate_Score12_IsModerated()
    {
        // age<65(3) + imc<30(1) + inactivo(2) + sinFrutas(1) + hta(2) + familiar2(3) = 12 → Moderado
        var p = FullProfile(age: 60, imc: 27, actividadFisica: false, frutas: false, hta: true, glucosaAlta: false, familiar1: false, familiar2: true);
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal(12, (int)result.Score);
        Assert.Equal("Moderado", result.Category);
    }

    [Fact]
    public void Calculate_Score21_IsMuyAlto()
    {
        // age>=65(4) + imc>=30(3) + inactivo(2) + sinFrutas(1) + hta(2) + glucosaAlta(5) + familiar1(5) = 22 → MuyAlto
        var p = FullProfile(age: 65, imc: 32, actividadFisica: false, frutas: false, hta: true, glucosaAlta: true, familiar1: true, familiar2: false);
        var result = FindriscAlgorithm.Calculate(p);
        Assert.True(result.Score >= 21);
        Assert.Equal("MuyAlto", result.Category);
    }

    // ── Perímetro abdominal ─────────────────────────────────────────────

    [Fact]
    public void Calculate_MalePerimetro94Plus_Adds3Points()
    {
        var withoutPerimetro = FullProfile(age: 40, imc: 24, actividadFisica: true, frutas: true,
            hta: false, glucosaAlta: false, familiar1: false, familiar2: false, perimetro: null);
        var withPerimetro = FullProfile(age: 40, imc: 24, actividadFisica: true, frutas: true,
            hta: false, glucosaAlta: false, familiar1: false, familiar2: false, perimetro: 95);

        var r1 = FindriscAlgorithm.Calculate(withoutPerimetro);
        var r2 = FindriscAlgorithm.Calculate(withPerimetro);
        Assert.Equal(3, (int)(r2.Score - r1.Score));
    }

    [Fact]
    public void Calculate_FemalePerimetro80Plus_Adds3Points()
    {
        var withoutPerimetro = FullProfile(age: 40, imc: 24, actividadFisica: true, frutas: true,
            hta: false, glucosaAlta: false, familiar1: false, familiar2: false, sex: "Female", perimetro: null);
        var withPerimetro = FullProfile(age: 40, imc: 24, actividadFisica: true, frutas: true,
            hta: false, glucosaAlta: false, familiar1: false, familiar2: false, sex: "Female", perimetro: 85);

        var r1 = FindriscAlgorithm.Calculate(withoutPerimetro);
        var r2 = FindriscAlgorithm.Calculate(withPerimetro);
        Assert.Equal(3, (int)(r2.Score - r1.Score));
    }

    // ── Familiar 1° vs 2° ───────────────────────────────────────────────

    [Fact]
    public void Calculate_Familiar1Grado_Adds5Points()
    {
        var pBase = FullProfile(familiar1: false, familiar2: false);
        var pF1   = FullProfile(familiar1: true,  familiar2: false);
        Assert.Equal(5, (int)(FindriscAlgorithm.Calculate(pF1).Score
                             - FindriscAlgorithm.Calculate(pBase).Score));
    }

    [Fact]
    public void Calculate_Familiar2Grado_Adds3Points()
    {
        var pBase = FullProfile(familiar1: false, familiar2: false);
        var pF2   = FullProfile(familiar1: false, familiar2: true);
        Assert.Equal(3, (int)(FindriscAlgorithm.Calculate(pF2).Score
                             - FindriscAlgorithm.Calculate(pBase).Score));
    }

    [Fact]
    public void Calculate_Familiar1And2Grado_Uses1GradoOnly()
    {
        // Ambos presentes: solo debe sumar +5 (1°), no +8
        var pBase = FullProfile(familiar1: false, familiar2: false);
        var pBoth = FullProfile(familiar1: true,  familiar2: true);
        Assert.Equal(5, (int)(FindriscAlgorithm.Calculate(pBoth).Score
                             - FindriscAlgorithm.Calculate(pBase).Score));
    }

    // ── Confianza ──────────────────────────────────────────────────────

    [Fact]
    public void Calculate_AllRequired_HasMaxConfidence()
    {
        var result = FindriscAlgorithm.Calculate(FullProfile());
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public void Calculate_AlgorithmName_IsFINDRISC()
    {
        var result = FindriscAlgorithm.Calculate(FullProfile());
        Assert.Equal("FINDRISC", result.Algorithm);
    }

    // ── Datos insuficientes ─────────────────────────────────────────────

    [Fact]
    public void Calculate_EmptyProfile_ReturnsInsuficienteDatos()
    {
        var p = new ConsolidatedPatientProfile { Age = 0, Sex = "Male" };
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal("InsuficienteDatos", result.Category);
    }

    [Fact]
    public void Calculate_OnlyAge_ReturnsInsuficienteDatos()
    {
        // Solo 1 de 7 vars → confianza 0.14 < 0.5
        var p = new ConsolidatedPatientProfile { Age = 45, Sex = "Male" };
        var result = FindriscAlgorithm.Calculate(p);
        Assert.Equal("InsuficienteDatos", result.Category);
        Assert.True(result.Confidence < 0.5);
    }

    [Fact]
    public void Calculate_HalfVars_ConfidenceIsAtLeast0Point5()
    {
        // age(1) + imc(1) + actividadFisica(1) + frutas(1) = 4/7 ≈ 0.57 → suficiente
        var p = new ConsolidatedPatientProfile
        {
            Age = 50, Sex = "Male",
            Imc = 26,
            ActividadFisicaDiaria = true,
            ConsumeFrutasVerduras = true
        };
        var result = FindriscAlgorithm.Calculate(p);
        Assert.True(result.Confidence >= 0.5);
        Assert.NotEqual("InsuficienteDatos", result.Category);
    }
}
