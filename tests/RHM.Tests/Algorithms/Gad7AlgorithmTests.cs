using RHM.Application.DTOs.Risk;
using RHM.Infrastructure.Services.Algorithms;

namespace RHM.Tests.Algorithms;

public class Gad7AlgorithmTests
{
    private static ConsolidatedPatientProfile ProfileWith(int[] items) =>
        new() { Gad7Items = items, Age = 40, Sex = "Male" };

    // ── Categorías ────────────────────────────────────────────────────

    [Theory]
    [InlineData(new[] { 0, 0, 0, 0, 0, 0, 0 }, "Minimo")]   // 0 pts
    [InlineData(new[] { 1, 1, 1, 1, 0, 0, 0 }, "Minimo")]   // 4 pts
    [InlineData(new[] { 1, 1, 1, 1, 1, 0, 0 }, "Leve")]     // 5 pts
    [InlineData(new[] { 1, 1, 1, 1, 1, 1, 1 }, "Leve")]     // 7 pts
    [InlineData(new[] { 2, 2, 1, 1, 1, 1, 1 }, "Leve")]     // 9 pts
    [InlineData(new[] { 2, 2, 2, 1, 1, 1, 1 }, "Moderado")] // 10 pts
    [InlineData(new[] { 2, 2, 2, 2, 2, 2, 0 }, "Moderado")] // 12 pts
    [InlineData(new[] { 2, 2, 2, 2, 2, 2, 2 }, "Moderado")] // 14 pts
    [InlineData(new[] { 3, 3, 3, 3, 1, 0, 0 }, "Moderado")]  // 13 pts → ≤14 = Moderado
    [InlineData(new[] { 3, 3, 3, 3, 2, 0, 0 }, "Moderado")] // 14 pts → ≤14 = Moderado
    [InlineData(new[] { 3, 3, 3, 3, 3, 0, 0 }, "Grave")]    // 15 pts → Grave
    [InlineData(new[] { 3, 3, 3, 3, 3, 3, 3 }, "Grave")]    // 21 pts
    public void Calculate_ReturnsCorrectCategory(int[] items, string expectedCategory)
    {
        var result = Gad7Algorithm.Calculate(ProfileWith(items));
        Assert.Equal(expectedCategory, result.Category);
    }

    [Fact]
    public void Calculate_Score_IsCorrectSum()
    {
        var items = new[] { 1, 2, 3, 0, 1, 2, 0 }; // 9 pts
        var result = Gad7Algorithm.Calculate(ProfileWith(items));
        Assert.Equal(9, result.Score);
    }

    [Fact]
    public void Calculate_FullData_HasMaxConfidence()
    {
        var result = Gad7Algorithm.Calculate(ProfileWith([0, 0, 0, 0, 0, 0, 0]));
        Assert.Equal(1.0, result.Confidence);
    }

    // ── Datos insuficientes ───────────────────────────────────────────

    [Fact]
    public void Calculate_NullItems_ReturnsInsuficienteDatos()
    {
        var p = new ConsolidatedPatientProfile { Age = 30, Sex = "Female" };
        var result = Gad7Algorithm.Calculate(p);
        Assert.Equal("InsuficienteDatos", result.Category);
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void Calculate_PartialItems_ReturnsInsuficienteDatos()
    {
        var result = Gad7Algorithm.Calculate(ProfileWith([2, 1, 0])); // 3 de 7
        Assert.Equal("InsuficienteDatos", result.Category);
    }

    // ── Inputs registrados ────────────────────────────────────────────

    [Fact]
    public void Calculate_Inputs_ContainsAllItems()
    {
        var result = Gad7Algorithm.Calculate(ProfileWith([1, 2, 3, 0, 1, 2, 1]));
        for (int i = 1; i <= 7; i++)
            Assert.True(result.Inputs.ContainsKey($"gad7_item_{i}"),
                $"Falta clave gad7_item_{i} en Inputs");
        Assert.True(result.Inputs.ContainsKey("total"));
    }
}
