using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RHM.Application.DTOs.Forms;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class FormService : IFormService
{
    private readonly MongoDbContext _mongo;
    private readonly QrService _qrService;
    private readonly IConfiguration _config;

    public FormService(MongoDbContext mongo, QrService qrService, IConfiguration config)
    {
        _mongo = mongo;
        _qrService = qrService;
        _config = config;
    }

    public async Task<IEnumerable<FormSchemaDto>> GetByTenantAsync(string tenantId)
    {
        var schemas = await _mongo.FormSchemas
            .Find(f => f.TenantId == tenantId)
            .SortByDescending(f => f.CreatedAt)
            .ToListAsync();
        return schemas.Select(MapToDto);
    }

    public async Task<FormSchemaDto?> GetByIdAsync(string id)
    {
        var schema = await _mongo.FormSchemas.Find(f => f.Id == id).FirstOrDefaultAsync();
        return schema is null ? null : MapToDto(schema);
    }

    public async Task<FormSchemaDto?> GetByPublicUrlAsync(string publicUrl)
    {
        var schema = await _mongo.FormSchemas
            .Find(f => f.PublicUrl == publicUrl && f.IsPublished)
            .FirstOrDefaultAsync();
        return schema is null ? null : MapToDto(schema);
    }

    public async Task<FormSchemaDto> CreateAsync(FormSchemaDto dto)
    {
        var schema = new FormSchema
        {
            TenantId = dto.TenantId,
            Title = dto.Title,
            Description = dto.Description,
            Fields = MapFields(dto.Fields),
            CreatedAt = DateTime.UtcNow
        };
        await _mongo.FormSchemas.InsertOneAsync(schema);
        return MapToDto(schema);
    }

    public async Task<FormSchemaDto> UpdateAsync(string id, FormSchemaDto dto)
    {
        var update = Builders<FormSchema>.Update
            .Set(f => f.Title, dto.Title)
            .Set(f => f.Description, dto.Description)
            .Set(f => f.Fields, MapFields(dto.Fields))
            .Set(f => f.UpdatedAt, DateTime.UtcNow);

        await _mongo.FormSchemas.UpdateOneAsync(f => f.Id == id, update);
        return (await GetByIdAsync(id))!;
    }

    public async Task<FormSchemaDto> PublishAsync(string id)
    {
        var schema = await _mongo.FormSchemas.Find(f => f.Id == id).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Formulario no encontrado.");

        var publicUrl = schema.PublicUrl ?? $"f{Guid.NewGuid():N}"[..12];
        var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:4200";
        var qrBase64 = _qrService.GenerateQrBase64($"{frontendUrl}/f/{publicUrl}");

        var update = Builders<FormSchema>.Update
            .Set(f => f.IsPublished, true)
            .Set(f => f.PublicUrl, publicUrl)
            .Set(f => f.QrCodeBase64, qrBase64)
            .Set(f => f.PublishedAt, DateTime.UtcNow);

        await _mongo.FormSchemas.UpdateOneAsync(f => f.Id == id, update);
        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(string id)
    {
        await _mongo.FormSchemas.DeleteOneAsync(f => f.Id == id);
    }

    private static List<FormField> MapFields(List<FormFieldDto> fields) =>
        fields.Select((f, i) => new FormField
        {
            Id = string.IsNullOrEmpty(f.Id) ? Guid.NewGuid().ToString() : f.Id,
            Type = f.Type,
            Label = f.Label,
            IsRequired = f.IsRequired,
            Options = f.Options,
            Order = i,
            Placeholder = f.Placeholder,
            // Validation
            MinValue = f.MinValue,
            MaxValue = f.MaxValue,
            MaxLength = f.MaxLength,
            Pattern = f.Pattern,
            DisableFutureDates = f.DisableFutureDates,
            // Conditional display
            ShowIf = f.ShowIf is null ? null : new ShowIfCondition
            {
                FieldId = f.ShowIf.FieldId,
                Operator = f.ShowIf.Operator,
                Value = f.ShowIf.Value
            },
            // Likert scale
            ScaleMin = f.ScaleMin,
            ScaleMax = f.ScaleMax,
            ScaleMinLabel = f.ScaleMinLabel,
            ScaleMaxLabel = f.ScaleMaxLabel,
            // Cascading dropdown
            ParentFieldId = f.ParentFieldId,
            CascadeOptions = f.CascadeOptions,
            // Section separator
            SectionDescription = f.SectionDescription
        }).ToList();

    private static FormSchemaDto MapToDto(FormSchema s) => new()
    {
        Id = s.Id,
        TenantId = s.TenantId,
        Title = s.Title,
        Description = s.Description,
        Fields = s.Fields.OrderBy(f => f.Order).Select(f => new FormFieldDto
        {
            Id = f.Id,
            Type = f.Type,
            Label = f.Label,
            IsRequired = f.IsRequired,
            Options = f.Options,
            Order = f.Order,
            Placeholder = f.Placeholder,
            // Validation
            MinValue = f.MinValue,
            MaxValue = f.MaxValue,
            MaxLength = f.MaxLength,
            Pattern = f.Pattern,
            DisableFutureDates = f.DisableFutureDates,
            // Conditional display
            ShowIf = f.ShowIf is null ? null : new ShowIfConditionDto
            {
                FieldId = f.ShowIf.FieldId,
                Operator = f.ShowIf.Operator,
                Value = f.ShowIf.Value
            },
            // Likert scale
            ScaleMin = f.ScaleMin,
            ScaleMax = f.ScaleMax,
            ScaleMinLabel = f.ScaleMinLabel,
            ScaleMaxLabel = f.ScaleMaxLabel,
            // Cascading dropdown
            ParentFieldId = f.ParentFieldId,
            CascadeOptions = f.CascadeOptions,
            // Section separator
            SectionDescription = f.SectionDescription
        }).ToList(),
        IsPublished = s.IsPublished,
        QrCodeBase64 = s.QrCodeBase64,
        PublicUrl = s.PublicUrl,
        CreatedAt = s.CreatedAt,
        PublishedAt = s.PublishedAt
    };
}
