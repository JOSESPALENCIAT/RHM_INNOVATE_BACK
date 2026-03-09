using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RHM.Application.DTOs.Risk;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

/// <summary>
/// Genera narrativas clínicas en lenguaje natural usando Claude API (claude-opus-4-6).
/// Persiste el texto generado en el campo AiNarrative del PatientRiskProfile más reciente.
/// </summary>
public class NarrativeService : INarrativeService
{
    private readonly MongoDbContext _mongo;
    private readonly HttpClient _http;
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NarrativeService(MongoDbContext mongo, IHttpClientFactory factory, IConfiguration config)
    {
        _mongo  = mongo;
        _http   = factory.CreateClient("claude");
        _apiKey = config["Claude:ApiKey"] ?? string.Empty;
    }

    public async Task<NarrativeResponseDto> GenerateAsync(
        string tenantId, string patientId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "La generación de narrativas clínicas requiere configurar 'Claude:ApiKey' en el App Service.");

        // 1. Obtener el perfil de riesgo más reciente del paciente
        var profile = await _mongo.PatientRiskProfiles
            .Find(r => r.TenantId == tenantId && r.PatientId == patientId)
            .SortByDescending(r => r.CalculatedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException(
                $"No existe perfil de riesgo para el paciente '{patientId}' en el tenant '{tenantId}'.");

        // 2. Construir el prompt con el perfil
        var prompt = BuildPrompt(profile);

        // 3. Llamar Claude API
        var narrative = await CallClaudeAsync(prompt, ct);

        // 4. Persistir en MongoDB
        var update = Builders<PatientRiskProfile>.Update.Set(r => r.AiNarrative, narrative);
        await _mongo.PatientRiskProfiles.UpdateOneAsync(
            r => r.Id == profile.Id, update, cancellationToken: ct);

        return new NarrativeResponseDto
        {
            PatientId   = patientId,
            ProfileId   = profile.Id,
            Narrative   = narrative,
            GeneratedAt = DateTime.UtcNow
        };
    }

    // ------------------------------------------------------------------ //
    //  CONSTRUCCIÓN DEL PROMPT                                            //
    // ------------------------------------------------------------------ //

    private static string BuildPrompt(PatientRiskProfile p)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Eres un médico clínico especialista en salud pública de Colombia.");
        sb.AppendLine("Redacta una narrativa clínica concisa (máximo 200 palabras) en español");
        sb.AppendLine("para el equipo de salud de la Entidad Territorial, basada en el siguiente");
        sb.AppendLine("perfil de riesgo del paciente. El tono debe ser profesional y orientado a la acción.");
        sb.AppendLine();
        sb.AppendLine("--- PERFIL DE RIESGO ---");
        sb.AppendLine($"Fecha del cálculo: {p.CalculatedAt:dd/MM/yyyy HH:mm} UTC");
        sb.AppendLine($"Submissions considerados: {p.DataSource.SubmissionsConsidered}");

        if (p.DataSource.MissingCriticalVars.Count > 0)
            sb.AppendLine($"Variables críticas faltantes: {string.Join(", ", p.DataSource.MissingCriticalVars)}");

        sb.AppendLine();
        sb.AppendLine($"RIESGO COMPUESTO: {p.CompositeRisk.Category} (dimensión dominante: {p.CompositeRisk.Dominant})");
        sb.AppendLine();

        AppendScore(sb, "Cardiovascular (Framingham)", p.Scores.Cardiovascular);
        AppendScore(sb, "Metabólico (FINDRISC)", p.Scores.Metabolic);

        if (p.Scores.Mental is not null)
        {
            AppendScore(sb, "Salud Mental — PHQ-9 (Depresión)", p.Scores.Mental.Phq9);
            AppendScore(sb, "Salud Mental — GAD-7 (Ansiedad)", p.Scores.Mental.Gad7);
        }

        if (p.Scores.Oncological is not null)
        {
            sb.AppendLine($"Oncológico: {p.Scores.Oncological.Category}" +
                          $" | Confianza: {p.Scores.Oncological.Confidence:P0}");
            if (p.Scores.Oncological.RedFlags.Count > 0)
                sb.AppendLine($"  Banderas rojas: {string.Join(", ", p.Scores.Oncological.RedFlags)}");
        }

        sb.AppendLine();
        sb.AppendLine("Genera la narrativa clínica ahora. No incluyas encabezados ni listas;" +
                      " solo párrafos de texto corrido.");

        return sb.ToString();
    }

    private static void AppendScore(StringBuilder sb, string label, AlgorithmScore? score)
    {
        if (score is null || score.Category is "Skipped") return;
        sb.AppendLine($"{label}: {score.Category} (score={score.Score:F1}, confianza={score.Confidence:P0})");
    }

    // ------------------------------------------------------------------ //
    //  LLAMADA A CLAUDE API                                               //
    // ------------------------------------------------------------------ //

    private async Task<string> CallClaudeAsync(string prompt, CancellationToken ct)
    {
        var requestBody = new
        {
            model      = "claude-opus-4-6",
            max_tokens = 1024,
            messages   = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody, _json),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = content;

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(raw);

        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}
