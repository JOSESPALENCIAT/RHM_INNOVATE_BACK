using MongoDB.Driver;
using RHM.Application.Common;
using RHM.Application.DTOs.FieldMapping;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class FieldMappingService : IFieldMappingService
{
    private readonly MongoDbContext _mongo;
    private readonly IMasterPatientIndexService _mpi;

    public FieldMappingService(MongoDbContext mongo, IMasterPatientIndexService mpi)
    {
        _mongo = mongo;
        _mpi   = mpi;
    }

    // ------------------------------------------------------------------ //
    //  CRUD                                                               //
    // ------------------------------------------------------------------ //

    public async Task<FieldMappingDto?> GetByFormIdAsync(string tenantId, string formId)
    {
        var doc = await _mongo.FieldMappings
            .Find(m => m.FormId == formId && m.TenantId == tenantId)
            .FirstOrDefaultAsync();

        return doc is null ? null : ToDto(doc);
    }

    public async Task<FieldMappingDto> SaveAsync(string tenantId, string formId, SaveFieldMappingDto dto)
    {
        // Validar que los destinos son variables clínicas conocidas
        var invalid = dto.Mappings.Values
            .Where(v => !string.IsNullOrWhiteSpace(v) && !ClinicalVariables.All.Contains(v))
            .Distinct()
            .ToList();

        if (invalid.Any())
            throw new ArgumentException(
                $"Variables clínicas no reconocidas: {string.Join(", ", invalid)}. " +
                $"Use una de las variables sys_* del catálogo.");

        var update = Builders<FieldMappingConfig>.Update
            .Set(m => m.FormId,    formId)
            .Set(m => m.TenantId,  tenantId)
            .Set(m => m.Mappings,  dto.Mappings)
            .Set(m => m.UpdatedAt, DateTime.UtcNow);

        var opts = new FindOneAndUpdateOptions<FieldMappingConfig>
        {
            IsUpsert       = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _mongo.FieldMappings.FindOneAndUpdateAsync(
            m => m.FormId == formId && m.TenantId == tenantId, update, opts);

        return ToDto(result);
    }

    // ------------------------------------------------------------------ //
    //  APLICACIÓN DE MAPEOS (post-submit)                                //
    // ------------------------------------------------------------------ //

    public async Task<Dictionary<string, string>> ApplyMappingsAsync(
        string formId, Dictionary<string, string> data)
    {
        var config = await _mongo.FieldMappings
            .Find(m => m.FormId == formId)
            .FirstOrDefaultAsync();

        if (config is null || config.Mappings.Count == 0)
            return data;

        var enriched = new Dictionary<string, string>(data);
        foreach (var (fieldKey, clinicalVar) in config.Mappings)
        {
            // Solo mapear si el campo fuente existe y el destino sys_* no está ya asignado
            if (!string.IsNullOrWhiteSpace(clinicalVar)
                && data.TryGetValue(fieldKey, out var value)
                && !enriched.ContainsKey(clinicalVar))
            {
                enriched[clinicalVar] = value;
            }
        }

        return enriched;
    }

    // ------------------------------------------------------------------ //
    //  CALLBACK n8n                                                       //
    // ------------------------------------------------------------------ //

    public async Task<string?> ProcessN8nCallbackAsync(string tenantId, N8nCallbackDto dto)
    {
        // 1. Leer submission
        var submission = await _mongo.FormResponses
            .Find(r => r.Id == dto.SubmissionId && r.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (submission is null) return null;

        // 2. Construir update de MongoDB
        var updates = new List<UpdateDefinition<FormResponse>>
        {
            Builders<FormResponse>.Update
                .Set(r => r.N8nStatus.Normalized, true)
                .Set(r => r.N8nStatus.NormalizedAt, DateTime.UtcNow)
        };

        // 3. Inyectar campos normalizados en Data (solo los no existentes)
        if (dto.NormalizedFields is { Count: > 0 })
        {
            var newData = new Dictionary<string, string>(submission.Data);
            foreach (var (key, value) in dto.NormalizedFields)
            {
                if (ClinicalVariables.All.Contains(key) && !newData.ContainsKey(key))
                    newData[key] = value;
            }
            updates.Add(Builders<FormResponse>.Update.Set(r => r.Data, newData));
        }

        var combined = Builders<FormResponse>.Update.Combine(updates);
        await _mongo.FormResponses.UpdateOneAsync(
            r => r.Id == dto.SubmissionId, combined);

        // 4. Actualizar DIVIPOLA en Azure SQL si fue resuelto
        if (!string.IsNullOrWhiteSpace(dto.DivipolaMunCode)
            && !string.IsNullOrWhiteSpace(submission.PatientId))
        {
            await _mpi.UpdateDivipolaAsync(
                submission.PatientId,
                dto.DivipolaMunCode,
                dto.DivipolaDeptCode ?? dto.DivipolaMunCode[..2]);
        }

        return submission.PatientId;
    }

    // ------------------------------------------------------------------ //
    //  MAPPER                                                             //
    // ------------------------------------------------------------------ //

    private static FieldMappingDto ToDto(FieldMappingConfig doc) => new()
    {
        FormId    = doc.FormId,
        Mappings  = doc.Mappings,
        UpdatedAt = doc.UpdatedAt
    };
}
