using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RHM.Application.DTOs.Risk;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;
using RHM.Infrastructure.Services.Algorithms;

namespace RHM.Infrastructure.Services;

/// <summary>
/// Orquestador del Motor de Estratificación de Riesgo.
/// Flujo: Resolver patientId → Consolidar perfil → Ejecutar algoritmos → Persistir resultado.
/// </summary>
public class RiskEngineService : IRiskEngineService
{
    private readonly AppDbContext _db;
    private readonly MongoDbContext _mongo;
    private readonly ITenantRiskConfigService _riskConfig;

    public RiskEngineService(AppDbContext db, MongoDbContext mongo, ITenantRiskConfigService riskConfig)
    {
        _db         = db;
        _mongo      = mongo;
        _riskConfig = riskConfig;
    }

    // ------------------------------------------------------------------ //
    //  CÁLCULO PRINCIPAL                                                  //
    // ------------------------------------------------------------------ //

    public async Task<PatientRiskProfileDto> CalculateAsync(
        string tenantId, string submissionId, CancellationToken ct = default)
    {
        // 1. Obtener el submission para extraer el patientId
        var submission = await _mongo.FormResponses
            .Find(r => r.Id == submissionId && r.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);

        if (submission?.PatientId is null)
            throw new InvalidOperationException($"Submission '{submissionId}' no tiene patientId asociado.");

        return await ComputeAndPersist(tenantId, submission.PatientId, submissionId, ct);
    }

    public async Task<PatientRiskProfileDto> RecalculateForPatientAsync(
        string tenantId, string patientId, CancellationToken ct = default)
        => await ComputeAndPersist(tenantId, patientId, null, ct);

    // ------------------------------------------------------------------ //
    //  CONSULTAS                                                          //
    // ------------------------------------------------------------------ //

    public async Task<IEnumerable<PatientRiskProfileDto>> GetHistoryAsync(
        string tenantId, string patientId, int limit = 12)
    {
        var docs = await _mongo.PatientRiskProfiles
            .Find(r => r.TenantId == tenantId && r.PatientId == patientId)
            .SortByDescending(r => r.CalculatedAt)
            .Limit(limit)
            .ToListAsync();

        return docs.Select(MapToDto);
    }

    public async Task<IEnumerable<PatientRiskSummaryDto>> GetPopulationSummaryAsync(
        string tenantId, string? categoryFilter = null, int page = 1, int pageSize = 50)
    {
        // Obtener el último perfil de riesgo por paciente en este tenant
        var filter = Builders<PatientRiskProfile>.Filter.Eq(r => r.TenantId, tenantId);
        if (!string.IsNullOrEmpty(categoryFilter))
            filter &= Builders<PatientRiskProfile>.Filter
                .Eq(r => r.CompositeRisk.Category, categoryFilter);

        var latestProfiles = await _mongo.PatientRiskProfiles
            .Find(filter)
            .SortByDescending(r => r.CalculatedAt)
            .ToListAsync();

        // Deduplicar: mantener solo el más reciente por paciente
        var byPatient = latestProfiles
            .GroupBy(r => r.PatientId)
            .Select(g => g.First())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (!byPatient.Any()) return [];

        // Enriquecer con datos demográficos de Azure SQL
        var patientIds = byPatient.Select(r => Guid.Parse(r.PatientId)).ToList();
        var patients = await _db.Patients
            .Where(p => patientIds.Contains(p.Id))
            .Include(p => p.Tenant)
            .ToListAsync();

        var patientMap = patients.ToDictionary(p => p.Id.ToString());

        return byPatient.Select(profile =>
        {
            patientMap.TryGetValue(profile.PatientId, out var patient);
            return new PatientRiskSummaryDto
            {
                PatientId         = profile.PatientId,
                DocType           = patient?.DocType.ToString() ?? "",
                DocNumber         = patient?.DocNumber ?? "",
                FullName          = patient is null ? "" : $"{patient.FirstName} {patient.LastName}",
                Age               = patient?.Age ?? 0,
                Sex               = patient?.BiologicalSex.ToString() ?? "",
                Municipio         = patient?.DivipolaMunCode,
                CompositeCategory = profile.CompositeRisk.Category,
                DominantRisk      = profile.CompositeRisk.Dominant,
                LastCalculatedAt  = profile.CalculatedAt,
                TotalSubmissions  = profile.DataSource.SubmissionsConsidered
            };
        });
    }

    // ------------------------------------------------------------------ //
    //  NÚCLEO: CONSOLIDAR + CALCULAR + PERSISTIR                         //
    // ------------------------------------------------------------------ //

    private async Task<PatientRiskProfileDto> ComputeAndPersist(
        string tenantId, string patientId, string? triggeredBySubmissionId, CancellationToken ct)
    {
        // 1. Cargar configuración parametrizable del tenant
        var cfg = await _riskConfig.GetAsync(tenantId);

        // 2. Consolidar perfil desde SQL + MongoDB
        var profile = await BuildConsolidatedProfile(tenantId, patientId, ct);

        // 3. Ejecutar solo los algoritmos habilitados; deshabilitados → Skipped
        var cardiovascular = cfg.EnableFramingham
            ? FraminghamAlgorithm.Calculate(profile)
            : Skipped("Framingham");
        var metabolic = cfg.EnableFindrisc
            ? FindriscAlgorithm.Calculate(profile)
            : Skipped("FINDRISC");
        var phq9 = cfg.EnablePhq9
            ? Phq9Algorithm.Calculate(profile)
            : Skipped("PHQ-9");
        var gad7 = cfg.EnableGad7
            ? Gad7Algorithm.Calculate(profile)
            : Skipped("GAD-7");
        var oncological = cfg.EnableOncological
            ? OncologicalRedFlags.Evaluate(profile)
            : SkippedOncological();

        // 4. Calcular riesgo compuesto respetando pesos configurados
        Documents.CompositeWeights? weights = cfg.Weights is null ? null : new Documents.CompositeWeights
        {
            Cardiovascular = cfg.Weights.Cardiovascular,
            Metabolic      = cfg.Weights.Metabolic,
            Mental         = cfg.Weights.Mental,
            Oncological    = cfg.Weights.Oncological
        };
        var composite = DetermineCompositeRisk(cardiovascular, metabolic, phq9, gad7, oncological, weights);

        // 5. Identificar variables críticas faltantes
        var missingVars = GetMissingCriticalVars(profile);

        // 6. Persistir en MongoDB
        var doc = new PatientRiskProfile
        {
            PatientId              = patientId,
            TenantId               = tenantId,
            CalculatedAt           = DateTime.UtcNow,
            TriggeredBySubmissionId = triggeredBySubmissionId,
            DataSource = new DataSourceSummary
            {
                SubmissionsConsidered = profile.SubmissionsConsidered,
                OldestDataPoint       = profile.OldestDataPoint,
                MissingCriticalVars   = missingVars
            },
            Scores = new RiskScores
            {
                Cardiovascular = MapAlgorithmScore(cardiovascular),
                Metabolic      = MapAlgorithmScore(metabolic),
                Mental         = new MentalHealthScore
                {
                    Phq9 = MapAlgorithmScore(phq9),
                    Gad7 = MapAlgorithmScore(gad7)
                },
                Oncological = new OncologicalScore
                {
                    RedFlags   = oncological.RedFlags,
                    Category   = oncological.Category,
                    Confidence = oncological.Confidence
                }
            },
            CompositeRisk = new Documents.CompositeRisk
            {
                Category = composite.Category,
                Dominant = composite.Dominant
            }
        };

        await _mongo.PatientRiskProfiles.InsertOneAsync(doc, null, ct);

        // 7. Marcar el submission como calculado
        if (triggeredBySubmissionId is not null)
        {
            var update = Builders<FormResponse>.Update
                .Set(r => r.N8nStatus.RiskCalculated, true)
                .Set(r => r.N8nStatus.RiskCalculatedAt, DateTime.UtcNow);
            await _mongo.FormResponses.UpdateOneAsync(
                r => r.Id == triggeredBySubmissionId, update, null, ct);
        }

        return MapToDto(doc);
    }

    // ------------------------------------------------------------------ //
    //  CONSOLIDADOR DE PERFIL (SQL + MongoDB)                            //
    // ------------------------------------------------------------------ //

    private async Task<ConsolidatedPatientProfile> BuildConsolidatedProfile(
        string tenantId, string patientId, CancellationToken ct)
    {
        // Datos demográficos desde Azure SQL
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == Guid.Parse(patientId) && p.TenantId == Guid.Parse(tenantId), ct);

        var profile = new ConsolidatedPatientProfile
        {
            PatientId = patientId,
            TenantId  = tenantId,
            Age       = patient?.Age ?? 0,
            Sex       = patient?.BiologicalSex.ToString() ?? "Indeterminate",
            DivipolaMunCode = patient?.DivipolaMunCode
        };

        // Variables clínicas desde MongoDB (todos los submissions del paciente)
        var submissions = await _mongo.FormResponses
            .Find(r => r.PatientId == patientId && r.TenantId == tenantId)
            .SortByDescending(r => r.SubmittedAt)
            .ToListAsync(ct);

        profile.SubmissionsConsidered = submissions.Count;
        profile.OldestDataPoint       = submissions.LastOrDefault()?.SubmittedAt;

        // Consolidar: usar el valor más reciente disponible de cada variable
        foreach (var sub in submissions)
        {
            var d = sub.Data;
            TrySetDouble(d, "sys_imc",                  v => profile.Imc ??= v);
            TrySetDouble(d, "sys_perimetro_abdominal",  v => profile.PerimetroAbdominal ??= v);
            TrySetBool  (d, "sys_tabaquismo",            v => profile.Tabaquismo ??= v);
            TrySetBool  (d, "sys_diabetes",              v => profile.DiabetesDiagnostico ??= v);
            TrySetBool  (d, "sys_hta",                   v => profile.HipertensionDiagnostico ??= v);
            TrySetDouble(d, "sys_pas",                   v => profile.PresionArterialSistolica ??= v);
            TrySetBool  (d, "sys_tto_hta",               v => profile.TratamientoHipertension ??= v);
            TrySetBool  (d, "sys_actividad_fisica",      v => profile.ActividadFisicaDiaria ??= v);
            TrySetBool  (d, "sys_dieta_frutas",          v => profile.ConsumeFrutasVerduras ??= v);
            TrySetBool  (d, "sys_glucosa_alta",          v => profile.GlucosaAltaHistoria ??= v);
            TrySetBool  (d, "sys_diabetes_familiar_1",   v => profile.DiabetesFamiliar1Grado ??= v);
            TrySetBool  (d, "sys_diabetes_familiar_2",   v => profile.DiabetesFamiliar2Grado ??= v);
            TrySetBool  (d, "sys_ca_familiar",           v => profile.AntecedentesCancerFamiliar ??= v);
            TrySetDate  (d, "sys_ultima_citologia",      v => profile.UltimaCitologia ??= v);
            TrySetDate  (d, "sys_ultima_mamografia",     v => profile.UltimaMamografia ??= v);
            TrySetDate  (d, "sys_ultima_colonoscopia",   v => profile.UltimaColonoscopia ??= v);

            // PHQ-9: buscar el primer submission que tenga los 9 ítems completos
            if (profile.Phq9Items is null)
                profile.Phq9Items = TryExtractLikertItems(d, "sys_phq9_", 9);

            // GAD-7
            if (profile.Gad7Items is null)
                profile.Gad7Items = TryExtractLikertItems(d, "sys_gad7_", 7);
        }

        return profile;
    }

    // ------------------------------------------------------------------ //
    //  RIESGO COMPUESTO                                                   //
    // ------------------------------------------------------------------ //

    private static CompositeRiskDto DetermineCompositeRisk(
        AlgorithmScoreDto cardio, AlgorithmScoreDto metabolic,
        AlgorithmScoreDto phq9, AlgorithmScoreDto gad7, OncologicalScoreDto oncological,
        Documents.CompositeWeights? weights = null)
    {
        var candidates = new List<(string domain, string category)>
        {
            ("cardiovascular", cardio.Category),
            ("metabolic",      metabolic.Category),
            ("mental_phq9",    phq9.Category),
            ("mental_gad7",    gad7.Category),
            ("oncological",    oncological.Category)
        };

        // Excluir InsuficienteDatos y Skipped del compuesto
        var excluded = new HashSet<string> { "InsuficienteDatos", "Skipped" };
        var valid = candidates.Where(c => !excluded.Contains(c.category)).ToList();
        if (!valid.Any())
            return new CompositeRiskDto { Category = "InsuficienteDatos", Dominant = "N/A" };

        var order = new[] { "MuyAlto", "Alto", "Moderado", "LigeramenteElevado",
                            "ModeradamenteGrave", "Grave", "Leve", "Bajo", "Minimo" };

        // Si hay pesos configurados: calcular promedio ponderado numérico
        if (weights is not null)
        {
            var numericMap = new Dictionary<string, double>
            {
                ["MuyAlto"]            = 4, ["Alto"]            = 3,
                ["ModeradamenteGrave"] = 3, ["Grave"]           = 3,
                ["Moderado"]           = 2, ["LigeramenteElevado"] = 1.5,
                ["Leve"]               = 1, ["Bajo"]            = 0, ["Minimo"] = 0
            };

            double w(string domain) => domain switch
            {
                "cardiovascular" => weights.Cardiovascular,
                "metabolic"      => weights.Metabolic,
                "mental_phq9" or "mental_gad7" => weights.Mental / 2,
                "oncological"    => weights.Oncological,
                _                => 0
            };

            var weightedSum = valid.Sum(c => numericMap.GetValueOrDefault(c.category, 0) * w(c.domain));
            var totalWeight = valid.Sum(c => w(c.domain));
            var avg = totalWeight > 0 ? weightedSum / totalWeight : 0;

            var weightedCategory = avg switch
            {
                >= 3.5 => "MuyAlto",
                >= 2.5 => "Alto",
                >= 1.5 => "Moderado",
                >= 0.5 => "Bajo",
                _      => "Bajo"
            };
            var dominant2 = valid.OrderBy(c => Array.IndexOf(order, c.category)).First();
            return new CompositeRiskDto { Category = weightedCategory, Dominant = dominant2.domain };
        }

        // Sin pesos: usar la categoría más alta (comportamiento por defecto)
        var dominant = valid.OrderBy(c => Array.IndexOf(order, c.category)).First();

        // Normalizar categorías de salud mental al esquema estándar
        var normalizedCategory = dominant.category switch
        {
            "Grave" or "ModeradamenteGrave" => "Alto",
            "Moderado"                       => "Moderado",
            "LigeramenteElevado" or "Leve"  => "Bajo",
            _                               => dominant.category
        };

        return new CompositeRiskDto { Category = normalizedCategory, Dominant = dominant.domain };
    }

    /// <summary>Retorna un score marcado como Skipped cuando el algoritmo está deshabilitado por config.</summary>
    private static AlgorithmScoreDto Skipped(string algorithm) => new()
    {
        Algorithm  = algorithm,
        Score      = 0,
        Category   = "Skipped",
        Confidence = 0,
        Inputs     = new() { ["motivo"] = "Algoritmo deshabilitado en configuración del tenant" }
    };

    private static OncologicalScoreDto SkippedOncological() => new()
    {
        RedFlags   = [],
        Category   = "Skipped",
        Confidence = 0
    };

    // ------------------------------------------------------------------ //
    //  HELPERS                                                            //
    // ------------------------------------------------------------------ //

    private static List<string> GetMissingCriticalVars(ConsolidatedPatientProfile p)
    {
        var missing = new List<string>();
        if (p.Age == 0) missing.Add("edad");
        if (p.Imc is null) missing.Add("sys_imc");
        if (p.Tabaquismo is null) missing.Add("sys_tabaquismo");
        if (p.HipertensionDiagnostico is null) missing.Add("sys_hta");
        if (p.DiabetesDiagnostico is null) missing.Add("sys_diabetes");
        if (p.ActividadFisicaDiaria is null) missing.Add("sys_actividad_fisica");
        if (p.GlucosaAltaHistoria is null) missing.Add("sys_glucosa_alta");
        return missing;
    }

    private static void TrySetDouble(Dictionary<string, string> d, string key, Action<double> setter)
    {
        if (d.TryGetValue(key, out var v) && double.TryParse(v, out var result))
            setter(result);
    }

    private static void TrySetBool(Dictionary<string, string> d, string key, Action<bool> setter)
    {
        if (!d.TryGetValue(key, out var v)) return;
        var lower = v.ToLowerInvariant().Trim();
        if (lower is "true" or "sí" or "si" or "yes" or "1") setter(true);
        else if (lower is "false" or "no" or "0") setter(false);
    }

    private static void TrySetDate(Dictionary<string, string> d, string key, Action<DateTime> setter)
    {
        if (d.TryGetValue(key, out var v) && DateTime.TryParse(v, out var date))
            setter(date);
    }

    private static int[]? TryExtractLikertItems(Dictionary<string, string> d, string prefix, int count)
    {
        var items = new int[count];
        int found = 0;
        for (int i = 1; i <= count; i++)
        {
            if (d.TryGetValue($"{prefix}{i}", out var v) && int.TryParse(v, out var n))
            { items[i - 1] = n; found++; }
        }
        return found == count ? items : null;
    }

    // ------------------------------------------------------------------ //
    //  MAPPERS                                                            //
    // ------------------------------------------------------------------ //

    private static AlgorithmScore? MapAlgorithmScore(AlgorithmScoreDto dto) =>
        dto is null ? null : new AlgorithmScore
        {
            Algorithm  = dto.Algorithm,
            Score      = dto.Score,
            Category   = dto.Category,
            Confidence = dto.Confidence,
            Inputs     = dto.Inputs.ToDictionary(k => k.Key, v => (object)v.Value)
        };

    private static PatientRiskProfileDto MapToDto(PatientRiskProfile doc) => new()
    {
        Id                       = doc.Id,
        PatientId                = doc.PatientId,
        TenantId                 = doc.TenantId,
        CalculatedAt             = doc.CalculatedAt,
        TriggeredBySubmissionId  = doc.TriggeredBySubmissionId,
        AiNarrative              = doc.AiNarrative,
        DataSource = new DataSourceSummaryDto
        {
            SubmissionsConsidered = doc.DataSource.SubmissionsConsidered,
            OldestDataPoint       = doc.DataSource.OldestDataPoint,
            MissingCriticalVars   = doc.DataSource.MissingCriticalVars
        },
        Scores = new RiskScoresDto
        {
            Cardiovascular = MapScoreDto(doc.Scores.Cardiovascular),
            Metabolic      = MapScoreDto(doc.Scores.Metabolic),
            Mental = doc.Scores.Mental is null ? null : new MentalHealthScoreDto
            {
                Phq9 = MapScoreDto(doc.Scores.Mental.Phq9),
                Gad7 = MapScoreDto(doc.Scores.Mental.Gad7)
            },
            Oncological = doc.Scores.Oncological is null ? null : new OncologicalScoreDto
            {
                RedFlags   = doc.Scores.Oncological.RedFlags,
                Category   = doc.Scores.Oncological.Category,
                Confidence = doc.Scores.Oncological.Confidence
            }
        },
        CompositeRisk = new CompositeRiskDto
        {
            Category = doc.CompositeRisk.Category,
            Dominant = doc.CompositeRisk.Dominant
        }
    };

    private static AlgorithmScoreDto? MapScoreDto(AlgorithmScore? s) =>
        s is null ? null : new AlgorithmScoreDto
        {
            Algorithm  = s.Algorithm,
            Score      = s.Score,
            Category   = s.Category,
            Confidence = s.Confidence,
            Inputs     = s.Inputs.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "")
        };
}
