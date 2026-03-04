using MongoDB.Driver;
using RHM.Application.DTOs.Forms;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class FormResponseService : IFormResponseService
{
    private readonly MongoDbContext _mongo;

    public FormResponseService(MongoDbContext mongo) => _mongo = mongo;

    public async Task<IEnumerable<FormResponseDto>> GetByFormAsync(string formId)
    {
        var responses = await _mongo.FormResponses
            .Find(r => r.FormId == formId)
            .SortByDescending(r => r.SubmittedAt)
            .ToListAsync();

        return responses.Select(r => new FormResponseDto
        {
            Id = r.Id,
            FormId = r.FormId,
            TenantId = r.TenantId,
            Data = r.Data,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            SubmittedAt = r.SubmittedAt
        });
    }

    public async Task<FormResponseDto> SubmitAsync(string formId, string tenantId, SubmitFormDto dto, string? ipAddress)
    {
        var response = new FormResponse
        {
            FormId = formId,
            TenantId = tenantId,
            Data = dto.Data,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IpAddress = ipAddress,
            SubmittedAt = DateTime.UtcNow
        };

        await _mongo.FormResponses.InsertOneAsync(response);

        return new FormResponseDto
        {
            Id = response.Id,
            FormId = response.FormId,
            TenantId = response.TenantId,
            Data = response.Data,
            Latitude = response.Latitude,
            Longitude = response.Longitude,
            SubmittedAt = response.SubmittedAt
        };
    }
}
