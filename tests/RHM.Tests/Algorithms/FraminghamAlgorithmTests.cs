using RHM.Application.DTOs.Risk;
using RHM.Infrastructure.Services.Algorithms;

namespace RHM.Tests.Algorithms;

public class FraminghamAlgorithmTests
{
    // Helper: perfil masculino completo de bajo riesgo baseline
    private static ConsolidatedPatientProfile MaleProfile(
        int age = 45,
        double imc = 24,
        bool tabaquismo = false,
        bool diabetes = false,
        bool hta = false,
        bool? ttoHta = null,
        double? pas = null) => new()
    {
        Age = age,
        Sex = "Male",
        Imc = imc,
        Tabaquismo = tabaquismo,
        DiabetesDiagnostico = diabetes,
        HipertensionDiagnostico = hta,
        TratamientoHipertension = ttoHta,
        PresionArterialSistolica = pas
    };

    private static ConsolidatedPatientProfile FemaleProfile(
        int age = 45,
        double imc = 24,
        bool tabaquismo = false,
        bool diabetes = false,
        bool hta = false,
        bool? ttoHta = null,
        double? pas = null) => new()
    {
        Age = age,
        Sex = "Female",
        Imc = imc,
        Tabaquismo = tabaquismo,
        DiabetesDiagnostico = diabetes,
        HipertensionDiagnostico = hta,
        TratamientoHipertension = ttoHta,
        PresionArterialSistolica = pas
    };

    // ── Algoritmo y nombre ──────────────────────────────────────────────

    [Fact]
    public void Calculate_AlgorithmName_IsFramingham()
    {
        var result = FraminghamAlgorithm.Calculate(MaleProfile());
        Assert.Equal("Framingham", result.Algorithm);
    }

    // ── Categorías ──────────────────────────────────────────────────────

    // Male 40, IMC 24, sin factores → pts: age<45(-9+wait: age=40 maps <45→-9),
    // IMC<25→-3, sum=-12, risk=1% → Bajo
    [Fact]
    public void Calculate_YoungMaleNoFactors_IsBajo()
    {
        var p = MaleProfile(age: 38, imc: 22);
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.Equal("Bajo", result.Category);
        Assert.True(result.Score < 10);
    }

    // Male 65, IMC 35, fumador, diabético, HTA con tto → alto riesgo
    [Fact]
    public void Calculate_OldMaleAllFactors_IsAltoOrMuyAlto()
    {
        var p = MaleProfile(age: 65, imc: 35, tabaquismo: true, diabetes: true, hta: true, ttoHta: true);
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.True(result.Category is "Alto" or "MuyAlto");
        Assert.True(result.Score >= 20);
    }

    // Female 35, IMC 22, sin factores → muy bajo riesgo
    [Fact]
    public void Calculate_YoungFemaleNoFactors_IsBajo()
    {
        var p = FemaleProfile(age: 35, imc: 22);
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.Equal("Bajo", result.Category);
        Assert.True(result.Score < 10);
    }

    [Fact]
    public void Calculate_OldFemaleAllFactors_IsAltoOrMuyAlto()
    {
        var p = FemaleProfile(age: 70, imc: 35, tabaquismo: true, diabetes: true, hta: true, ttoHta: true);
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.True(result.Category is "Alto" or "MuyAlto");
    }

    // ── Efecto individual de cada factor (variar uno a la vez) ──────────

    [Fact]
    public void Calculate_TabaquismoMale_IncreasesScore()
    {
        var noTabaco = MaleProfile(age: 50, imc: 27);
        var conTabaco = MaleProfile(age: 50, imc: 27, tabaquismo: true);
        Assert.True(FraminghamAlgorithm.Calculate(conTabaco).Score >
                    FraminghamAlgorithm.Calculate(noTabaco).Score);
    }

    [Fact]
    public void Calculate_DiabetesMale_IncreasesScore()
    {
        var noDm = MaleProfile(age: 50, imc: 27);
        var conDm = MaleProfile(age: 50, imc: 27, diabetes: true);
        Assert.True(FraminghamAlgorithm.Calculate(conDm).Score >
                    FraminghamAlgorithm.Calculate(noDm).Score);
    }

    [Fact]
    public void Calculate_HtaConTratamiento_HigherThanSinTratamiento_Male()
    {
        var sinTto = MaleProfile(age: 55, imc: 27, hta: true, ttoHta: false);
        var conTto = MaleProfile(age: 55, imc: 27, hta: true, ttoHta: true);
        Assert.True(FraminghamAlgorithm.Calculate(conTto).Score >=
                    FraminghamAlgorithm.Calculate(sinTto).Score);
    }

    [Fact]
    public void Calculate_ObeseMale_HigherThanNormalImc()
    {
        var normal = MaleProfile(age: 50, imc: 22);
        var obese  = MaleProfile(age: 50, imc: 35);
        Assert.True(FraminghamAlgorithm.Calculate(obese).Score >
                    FraminghamAlgorithm.Calculate(normal).Score);
    }

    // ── Confianza ──────────────────────────────────────────────────────

    [Fact]
    public void Calculate_AllRequiredVarsPresent_HasMaxConfidence()
    {
        // 6 vars: Age, Sex, Tabaquismo, HipertensionDiagnostico, DiabetesDiagnostico, Imc
        var p = MaleProfile(age: 50, imc: 27, tabaquismo: false, diabetes: false, hta: false);
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public void Calculate_InputsContainsExpectedKeys()
    {
        var result = FraminghamAlgorithm.Calculate(MaleProfile());
        Assert.True(result.Inputs.ContainsKey("edad"));
        Assert.True(result.Inputs.ContainsKey("sexo"));
        Assert.True(result.Inputs.ContainsKey("imc"));
        Assert.True(result.Inputs.ContainsKey("tabaquismo"));
        Assert.True(result.Inputs.ContainsKey("diabetes"));
        Assert.True(result.Inputs.ContainsKey("hipertension"));
    }

    // ── Datos insuficientes ─────────────────────────────────────────────

    [Fact]
    public void Calculate_EmptyProfile_ReturnsInsuficienteDatos()
    {
        var p = new ConsolidatedPatientProfile { Age = 0, Sex = string.Empty };
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.Equal("InsuficienteDatos", result.Category);
    }

    [Fact]
    public void Calculate_OnlyAge_ReturnsInsuficienteDatos()
    {
        // Solo 1 de 6 vars → confianza ≈ 0.17 < 0.5
        var p = new ConsolidatedPatientProfile { Age = 50, Sex = "Male" };
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.Equal("InsuficienteDatos", result.Category);
        Assert.True(result.Confidence < 0.5);
    }

    [Fact]
    public void Calculate_ThreeVars_ConfidenceIs0Point5()
    {
        // age(1) + sex(1) + imc(1) = 3/6 = 0.5 → NOT < 0.5 → should calculate
        var p = new ConsolidatedPatientProfile { Age = 50, Sex = "Male", Imc = 26 };
        var result = FraminghamAlgorithm.Calculate(p);
        Assert.Equal(0.5, result.Confidence);
        Assert.NotEqual("InsuficienteDatos", result.Category);
    }

    // ── PAS como factor adicional ───────────────────────────────────────

    [Fact]
    public void Calculate_HighPas_HigherScoreThanLowPas()
    {
        var lowPas  = MaleProfile(age: 55, imc: 27, pas: 110);
        var highPas = MaleProfile(age: 55, imc: 27, pas: 165);
        Assert.True(FraminghamAlgorithm.Calculate(highPas).Score >
                    FraminghamAlgorithm.Calculate(lowPas).Score);
    }

    // ── Score nunca negativo para perfil típico ─────────────────────────

    [Fact]
    public void Calculate_ScoreIsAlwaysPositive()
    {
        // Riesgo se convierte a % mediante PointsToRisk → siempre >= 1%
        var result = FraminghamAlgorithm.Calculate(MaleProfile(age: 30, imc: 20));
        Assert.True(result.Score >= 1);
    }
}
