using MongoDB.Driver;
using RHM.Application.DTOs.Risk;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class TenantRiskConfigService : ITenantRiskConfigService
{
    private readonly MongoDbContext _mongo;

    public TenantRiskConfigService(MongoDbContext mongo) => _mongo = mongo;

    // ------------------------------------------------------------------ //
    //  Lectura                                                            //
    // ------------------------------------------------------------------ //

    public async Task<TenantRiskConfigDto> GetAsync(string tenantId)
    {
        var doc = await FindOrDefault(tenantId);
        return ToDto(doc);
    }

    // ------------------------------------------------------------------ //
    //  Escritura (upsert)                                                //
    // ------------------------------------------------------------------ //

    public async Task<TenantRiskConfigDto> SaveAsync(string tenantId, SaveTenantRiskConfigDto dto)
    {
        // Validar pesos si se proporcionan
        if (dto.Weights is not null)
        {
            var sum = dto.Weights.Cardiovascular + dto.Weights.Metabolic
                    + dto.Weights.Mental         + dto.Weights.Oncological;
            if (Math.Abs(sum - 1.0) > 0.05)
                throw new ArgumentException(
                    $"Los pesos deben sumar 1.0 (suma actual: {sum:F2}). " +
                    "Ajuste Cardiovascular + Metabolico + Mental + Oncologico = 1.0");
        }

        // Validar umbrales en rango [0,1]
        if (dto.MinConfidenceThreshold is < 0 or > 1)
            throw new ArgumentException("MinConfidenceThreshold debe estar entre 0.0 y 1.0");
        if (dto.DataCompletenessAlertThreshold is < 0 or > 1)
            throw new ArgumentException("DataCompletenessAlertThreshold debe estar entre 0.0 y 1.0");

        var update = Builders<TenantRiskConfig>.Update
            .Set(c => c.TenantId,                        tenantId)
            .Set(c => c.EnableFramingham,                 dto.EnableFramingham)
            .Set(c => c.EnableFindrisc,                   dto.EnableFindrisc)
            .Set(c => c.EnablePhq9,                       dto.EnablePhq9)
            .Set(c => c.EnableGad7,                       dto.EnableGad7)
            .Set(c => c.EnableOncological,                dto.EnableOncological)
            .Set(c => c.MinConfidenceThreshold,           dto.MinConfidenceThreshold)
            .Set(c => c.DataCompletenessAlertThreshold,   dto.DataCompletenessAlertThreshold)
            .Set(c => c.Weights,                          dto.Weights is null ? null : new CompositeWeights
            {
                Cardiovascular = dto.Weights.Cardiovascular,
                Metabolic      = dto.Weights.Metabolic,
                Mental         = dto.Weights.Mental,
                Oncological    = dto.Weights.Oncological
            })
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var opts = new FindOneAndUpdateOptions<TenantRiskConfig>
        {
            IsUpsert       = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _mongo.TenantRiskConfigs.FindOneAndUpdateAsync(
            c => c.TenantId == tenantId, update, opts);

        return ToDto(result);
    }

    // ------------------------------------------------------------------ //
    //  Helpers                                                            //
    // ------------------------------------------------------------------ //

    private async Task<TenantRiskConfig> FindOrDefault(string tenantId)
    {
        var doc = await _mongo.TenantRiskConfigs
            .Find(c => c.TenantId == tenantId)
            .FirstOrDefaultAsync();

        // Si no existe configuración guardada, retornar defaults sin persistir
        return doc ?? new TenantRiskConfig { TenantId = tenantId };
    }

    private static TenantRiskConfigDto ToDto(TenantRiskConfig doc) => new()
    {
        EnableFramingham               = doc.EnableFramingham,
        EnableFindrisc                 = doc.EnableFindrisc,
        EnablePhq9                     = doc.EnablePhq9,
        EnableGad7                     = doc.EnableGad7,
        EnableOncological              = doc.EnableOncological,
        MinConfidenceThreshold         = doc.MinConfidenceThreshold,
        DataCompletenessAlertThreshold = doc.DataCompletenessAlertThreshold,
        Weights = doc.Weights is null ? null : new CompositeWeightsDto
        {
            Cardiovascular = doc.Weights.Cardiovascular,
            Metabolic      = doc.Weights.Metabolic,
            Mental         = doc.Weights.Mental,
            Oncological    = doc.Weights.Oncological
        },
        UpdatedAt = doc.UpdatedAt == default ? null : doc.UpdatedAt
    };
}
