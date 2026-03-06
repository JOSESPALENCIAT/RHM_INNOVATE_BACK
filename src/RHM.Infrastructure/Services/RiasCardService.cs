using MongoDB.Driver;
using RHM.Application.DTOs.Forms;
using RHM.Application.DTOs.Rias;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class RiasCardService : IRiasCardService
{
    private readonly MongoDbContext _mongo;

    public RiasCardService(MongoDbContext mongo) => _mongo = mongo;

    public async Task<TenantRiasConfigDto?> GetAsync()
    {
        var doc = await _mongo.GlobalRiasConfig.Find(_ => true).FirstOrDefaultAsync();
        return doc is null ? null : MapToDto(doc);
    }

    public async Task<TenantRiasConfigDto> SaveAsync(string userId, TenantRiasConfigDto dto)
    {
        var existing = await _mongo.GlobalRiasConfig.Find(_ => true).FirstOrDefaultAsync();

        var doc = new GlobalRiasConfig
        {
            Id = existing?.Id ?? string.Empty,
            Cards = dto.Cards.Select(MapCard).ToList(),
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUserId = userId
        };

        if (existing is null)
            await _mongo.GlobalRiasConfig.InsertOneAsync(doc);
        else
            await _mongo.GlobalRiasConfig.ReplaceOneAsync(d => d.Id == existing.Id, doc);

        return MapToDto(doc);
    }

    public async Task DeleteAsync()
    {
        await _mongo.GlobalRiasConfig.DeleteOneAsync(_ => true);
    }

    // --- Mapping helpers ---

    private static RiasCardDoc MapCard(RiasCardDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        Subtitle = dto.Subtitle,
        Color = dto.Color,
        Icon = dto.Icon,
        IsActive = dto.IsActive,
        Sections = dto.Sections.Select(MapSection).ToList()
    };

    private static RiasSectionDoc MapSection(RiasSectionDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        Description = dto.Description,
        IsActive = dto.IsActive,
        Fields = dto.Fields.Select(f => new FormField
        {
            Id = string.IsNullOrEmpty(f.Id) ? Guid.NewGuid().ToString() : f.Id,
            Type = f.Type,
            Label = f.Label,
            IsRequired = f.IsRequired,
            Options = f.Options,
            Order = f.Order,
            Placeholder = f.Placeholder,
            MinValue = f.MinValue,
            MaxValue = f.MaxValue,
            MaxLength = f.MaxLength,
            Pattern = f.Pattern,
            DisableFutureDates = f.DisableFutureDates,
            ShowIf = f.ShowIf is null ? null : new ShowIfCondition
            {
                FieldId = f.ShowIf.FieldId,
                Operator = f.ShowIf.Operator,
                Value = f.ShowIf.Value
            },
            ScaleMin = f.ScaleMin,
            ScaleMax = f.ScaleMax,
            ScaleMinLabel = f.ScaleMinLabel,
            ScaleMaxLabel = f.ScaleMaxLabel,
            ParentFieldId = f.ParentFieldId,
            CascadeOptions = f.CascadeOptions,
            SectionDescription = f.SectionDescription
        }).ToList()
    };

    private static TenantRiasConfigDto MapToDto(GlobalRiasConfig doc) => new()
    {
        UpdatedAt = doc.UpdatedAt,
        UpdatedByUserId = doc.UpdatedByUserId,
        Cards = doc.Cards.Select(c => new RiasCardDto
        {
            Id = c.Id,
            Title = c.Title,
            Subtitle = c.Subtitle,
            Color = c.Color,
            Icon = c.Icon,
            IsActive = c.IsActive,
            Sections = c.Sections.Select(s => new RiasSectionDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                IsActive = s.IsActive,
                Fields = s.Fields.Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    Type = f.Type,
                    Label = f.Label,
                    IsRequired = f.IsRequired,
                    Options = f.Options,
                    Order = f.Order,
                    Placeholder = f.Placeholder,
                    MinValue = f.MinValue,
                    MaxValue = f.MaxValue,
                    MaxLength = f.MaxLength,
                    Pattern = f.Pattern,
                    DisableFutureDates = f.DisableFutureDates,
                    ShowIf = f.ShowIf is null ? null : new ShowIfConditionDto
                    {
                        FieldId = f.ShowIf.FieldId,
                        Operator = f.ShowIf.Operator,
                        Value = f.ShowIf.Value
                    },
                    ScaleMin = f.ScaleMin,
                    ScaleMax = f.ScaleMax,
                    ScaleMinLabel = f.ScaleMinLabel,
                    ScaleMaxLabel = f.ScaleMaxLabel,
                    ParentFieldId = f.ParentFieldId,
                    CascadeOptions = f.CascadeOptions,
                    SectionDescription = f.SectionDescription
                }).ToList()
            }).ToList()
        }).ToList()
    };
}
