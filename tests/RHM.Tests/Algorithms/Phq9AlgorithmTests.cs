using RHM.Application.DTOs.Risk;
using RHM.Infrastructure.Services.Algorithms;

namespace RHM.Tests.Algorithms;

public class Phq9AlgorithmTests
{
    private static ConsolidatedPatientProfile ProfileWith(int[] items) =>
        new() { Phq9Items = items, Age = 35, Sex = "Female" };

    // ── Categorías ────────────────────────────────────────────────────

    [Theory]
    [InlineData(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, "Minimo")]   // 0 pts
    [InlineData(new[] { 1, 1, 1, 1, 0, 0, 0, 0, 0 }, "Minimo")]   // 4 pts
    [InlineData(new[] { 1, 1, 1, 1, 1, 0, 0, 0, 0 }, "Leve")]     // 5 pts
    [InlineData(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, "Leve")]     // 9 pts
    [InlineData(new[] { 2, 2, 1, 1, 1, 1, 1, 1, 0 }, "Moderado")] // 10 pts
    [InlineData(new[] { 2, 2, 2, 2, 1, 1, 1, 1, 0 }, "Moderado")] // 12 pts
    [InlineData(new[] { 2, 2, 2, 2, 2, 2, 1, 1, 0 }, "Moderado")]           // 14 pts → ≤14 = Moderado
    [InlineData(new[] { 2, 2, 2, 2, 2, 2, 2, 1, 0 }, "ModeradamenteGrave")] // 15 pts
    [InlineData(new[] { 3, 3, 3, 3, 3, 2, 2, 0, 0 }, "ModeradamenteGrave")] // 19 pts
    [InlineData(new[] { 3, 3, 3, 3, 3, 2, 2, 1, 0 }, "Grave")]    // 20 pts
    [InlineData(new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3 }, "Grave")]    // 27 pts
    public void Calculate_ReturnsCorrectCategory(int[] items, string expectedCategory)
    {
        var result = Phq9Algorithm.Calculate(ProfileWith(items));
        Assert.Equal(expectedCategory, result.Category);
    }

    [Fact]
    public void Calculate_FullResponse_HasMaxConfidence()
    {
        var result = Phq9Algorithm.Calculate(ProfileWith([1, 0, 0, 0, 0, 0, 0, 0, 0]));
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public void Calculate_Score_IsCorrectSum()
    {
        var items = new[] { 2, 3, 1, 0, 1, 2, 1, 1, 0 }; // sum=11
        var result = Phq9Algorithm.Calculate(ProfileWith(items));
        Assert.Equal(11, result.Score);
    }

    // ── Ítem 9 — Ideación suicida ─────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Calculate_Item9Positive_SetsAlertFlag(int item9Value)
    {
        var items = new int[9]; // 0,0,0...
        items[8] = item9Value;
        var result = Phq9Algorithm.Calculate(ProfileWith(items));
        Assert.True(result.Inputs.ContainsKey("alerta_ideacion_suicida"));
        Assert.Equal("true", result.Inputs["alerta_ideacion_suicida"]);
    }

    [Fact]
    public void Calculate_Item9Zero_NoAlertFlag()
    {
        var result = Phq9Algorithm.Calculate(ProfileWith([0, 0, 0, 0, 0, 0, 0, 0, 0]));
        Assert.False(result.Inputs.ContainsKey("alerta_ideacion_suicida"));
    }

    // ── Datos insuficientes ───────────────────────────────────────────

    [Fact]
    public void Calculate_NullItems_ReturnsInsuficienteDatos()
    {
        var p = new ConsolidatedPatientProfile { Age = 30, Sex = "Male" };
        var result = Phq9Algorithm.Calculate(p);
        Assert.Equal("InsuficienteDatos", result.Category);
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void Calculate_PartialItems_ReturnsInsuficienteDatos()
    {
        var result = Phq9Algorithm.Calculate(ProfileWith([1, 2, 0])); // solo 3 de 9
        Assert.Equal("InsuficienteDatos", result.Category);
        Assert.True(result.Confidence > 0 && result.Confidence < 1);
    }

    // ── Valores fuera de rango (clamping) ─────────────────────────────

    [Fact]
    public void Calculate_OutOfRangeValues_AreClamped()
    {
        var items = new[] { 99, -5, 0, 0, 0, 0, 0, 0, 0 }; // 99 → 3, -5 → 0
        var result = Phq9Algorithm.Calculate(ProfileWith(items));
        Assert.Equal(3, result.Score); // solo el primer ítem clampeado a 3
    }
}
