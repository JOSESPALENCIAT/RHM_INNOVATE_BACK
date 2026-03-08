using MongoDB.Driver;
using RHM.Application.DTOs.Forms;
using RHM.Application.DTOs.Patients;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Documents;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class FormResponseService : IFormResponseService
{
    private readonly MongoDbContext _mongo;
    private readonly IMasterPatientIndexService _mpi;
    private readonly IFieldMappingService _fieldMapping;

    public FormResponseService(
        MongoDbContext mongo,
        IMasterPatientIndexService mpi,
        IFieldMappingService fieldMapping)
    {
        _mongo        = mongo;
        _mpi          = mpi;
        _fieldMapping = fieldMapping;
    }

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
            PatientId = r.PatientId,
            Data = r.Data,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            SubmittedAt = r.SubmittedAt
        });
    }

    public async Task<FormResponseDto> SubmitAsync(string formId, string tenantId, SubmitFormDto dto, string? ipAddress)
    {
        // 1. Aplicar mappings: traducir campos del formulario a variables clínicas sys_*
        var enrichedData = dto.Data;
        try { enrichedData = await _fieldMapping.ApplyMappingsAsync(formId, dto.Data); }
        catch { /* No bloquear el submit si el mapeo falla */ }

        // 2. Intentar resolver el paciente en el MPI si el formulario incluye el bloque demográfico
        string? patientId = null;
        var resolveDto = TryExtractDemographics(enrichedData);
        if (resolveDto is not null)
        {
            try { patientId = await _mpi.ResolvePatientKeyAsync(tenantId, resolveDto); }
            catch { /* No bloquear el submit si el MPI falla */ }
        }

        var response = new FormResponse
        {
            FormId = formId,
            TenantId = tenantId,
            PatientId = patientId,
            Data = enrichedData,
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
            PatientId = response.PatientId,
            Data = response.Data,
            Latitude = response.Latitude,
            Longitude = response.Longitude,
            SubmittedAt = response.SubmittedAt
        };
    }

    /// <summary>
    /// Extrae los campos sys_* del Data del formulario para construir el ResolvePatientDto.
    /// Retorna null si faltan los campos mínimos obligatorios (DocType + DocNumber).
    /// </summary>
    private static ResolvePatientDto? TryExtractDemographics(Dictionary<string, string> data)
    {
        if (!data.TryGetValue("sys_doc_type", out var docType) || string.IsNullOrWhiteSpace(docType)) return null;
        if (!data.TryGetValue("sys_doc_number", out var docNumber) || string.IsNullOrWhiteSpace(docNumber)) return null;

        data.TryGetValue("sys_first_name", out var firstName);
        data.TryGetValue("sys_last_name", out var lastName);
        data.TryGetValue("sys_birth_date", out var birthDateStr);
        data.TryGetValue("sys_sex", out var sex);
        data.TryGetValue("sys_contact_phone", out var phone);
        data.TryGetValue("sys_contact_email", out var email);
        data.TryGetValue("sys_municipio", out var municipio);

        DateTime.TryParse(birthDateStr, out var birthDate);

        return new ResolvePatientDto
        {
            DocType    = docType,
            DocNumber  = docNumber,
            FirstName  = firstName ?? string.Empty,
            LastName   = lastName ?? string.Empty,
            BirthDate  = birthDate == default ? DateTime.UtcNow.AddYears(-30) : birthDate,
            BiologicalSex = sex ?? "Indeterminate",
            ContactPhone  = phone,
            ContactEmail  = email,
            Municipio     = municipio
        };
    }
}
