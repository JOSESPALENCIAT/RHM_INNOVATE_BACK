using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RHM.Application.DTOs.Patients;
using RHM.Application.Interfaces;
using RHM.Domain.Entities;
using RHM.Domain.Enums;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

/// <summary>
/// Resuelve la identidad del paciente en Azure SQL.
/// Llave natural Colombia: TenantId + TipoDoc + NumDoc.
/// Si no existe, crea la fila. Si existe, actualiza datos de contacto si cambiaron.
/// </summary>
public class MasterPatientIndexService : IMasterPatientIndexService
{
    private readonly AppDbContext _db;
    private readonly MongoDbContext _mongo;

    public MasterPatientIndexService(AppDbContext db, MongoDbContext mongo)
    {
        _db    = db;
        _mongo = mongo;
    }

    public async Task<string> ResolvePatientKeyAsync(string tenantId, ResolvePatientDto dto)
    {
        if (!Enum.TryParse<DocumentType>(dto.DocType, ignoreCase: true, out var docType))
            docType = DocumentType.CC;

        if (!Enum.TryParse<BiologicalSex>(dto.BiologicalSex, ignoreCase: true, out var sex))
            sex = BiologicalSex.Indeterminate;

        var tenantGuid = Guid.Parse(tenantId);
        var docNumber = dto.DocNumber.Trim();

        var existing = await _db.Patients
            .FirstOrDefaultAsync(p =>
                p.TenantId == tenantGuid &&
                p.DocType == docType &&
                p.DocNumber == docNumber);

        if (existing is null)
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = tenantGuid,
                DocType = docType,
                DocNumber = docNumber,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                BirthDate = dto.BirthDate,
                BiologicalSex = sex,
                ContactPhone = dto.ContactPhone?.Trim(),
                ContactEmail = dto.ContactEmail?.Trim().ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            return patient.Id.ToString();
        }

        // Actualizar datos de contacto si cambiaron
        var changed = false;

        if (!string.IsNullOrWhiteSpace(dto.ContactPhone) && dto.ContactPhone != existing.ContactPhone)
        { existing.ContactPhone = dto.ContactPhone.Trim(); changed = true; }

        if (!string.IsNullOrWhiteSpace(dto.ContactEmail))
        {
            var normalizedEmail = dto.ContactEmail.Trim().ToLowerInvariant();
            if (normalizedEmail != existing.ContactEmail)
            { existing.ContactEmail = normalizedEmail; changed = true; }
        }

        if (changed)
        {
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return existing.Id.ToString();
    }

    public async Task UpdateDivipolaAsync(string patientId, string munCode, string deptCode)
    {
        if (!Guid.TryParse(patientId, out var guid)) return;

        var patient = await _db.Patients.FindAsync(guid);
        if (patient is null) return;

        patient.DivipolaMunCode = munCode;
        patient.DivipolaDeptCode = deptCode;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ------------------------------------------------------------------ //
    //  DEDUPLICACIÓN                                                      //
    // ------------------------------------------------------------------ //

    public async Task<IEnumerable<DuplicateGroupDto>> GetDuplicatesAsync(string tenantId)
    {
        var tenantGuid = Guid.Parse(tenantId);

        var patients = await _db.Patients
            .Where(p => p.TenantId == tenantGuid)
            .ToListAsync();

        // Normalizar DocNumber: solo dígitos y letras, en mayúsculas
        var groups = patients
            .GroupBy(p => Normalize(p.DocNumber))
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateGroupDto
            {
                NormalizedKey = g.Key,
                Patients = g.Select(MapToDto).ToList()
            })
            .ToList();

        return groups;
    }

    public async Task<MergeResultDto> MergeAsync(string tenantId, MergePatientDto dto)
    {
        var tenantGuid = Guid.Parse(tenantId);

        var primary   = await _db.Patients.FindAsync(dto.PrimaryId)
            ?? throw new InvalidOperationException($"Paciente primario '{dto.PrimaryId}' no encontrado.");
        var secondary = await _db.Patients.FindAsync(dto.SecondaryId)
            ?? throw new InvalidOperationException($"Paciente secundario '{dto.SecondaryId}' no encontrado.");

        if (primary.TenantId != tenantGuid || secondary.TenantId != tenantGuid)
            throw new InvalidOperationException("Ambos pacientes deben pertenecer al mismo tenant.");

        var primaryIdStr   = dto.PrimaryId.ToString();
        var secondaryIdStr = dto.SecondaryId.ToString();

        // 1. Reasignar FormResponses en MongoDB
        var formFilter = Builders<Documents.FormResponse>.Filter.Eq(r => r.PatientId, secondaryIdStr);
        var formUpdate = Builders<Documents.FormResponse>.Update.Set(r => r.PatientId, primaryIdStr);
        var formResult = await _mongo.FormResponses.UpdateManyAsync(formFilter, formUpdate);

        // 2. Reasignar PatientRiskProfiles en MongoDB
        var riskFilter = Builders<Documents.PatientRiskProfile>.Filter.Eq(r => r.PatientId, secondaryIdStr);
        var riskUpdate = Builders<Documents.PatientRiskProfile>.Update.Set(r => r.PatientId, primaryIdStr);
        var riskResult = await _mongo.PatientRiskProfiles.UpdateManyAsync(riskFilter, riskUpdate);

        // 3. Eliminar el paciente secundario de Azure SQL
        _db.Patients.Remove(secondary);
        await _db.SaveChangesAsync();

        return new MergeResultDto
        {
            PrimaryId              = primaryIdStr,
            FormResponsesMigrated  = (int)formResult.ModifiedCount,
            RiskProfilesMigrated   = (int)riskResult.ModifiedCount,
            SecondaryDeleted       = true
        };
    }

    // ------------------------------------------------------------------ //
    //  HELPERS                                                            //
    // ------------------------------------------------------------------ //

    private static string Normalize(string docNumber) =>
        Regex.Replace(docNumber.ToUpperInvariant().Trim(), @"[^A-Z0-9]", "");

    private static PatientDto MapToDto(Patient p) => new()
    {
        Id              = p.Id,
        DocType         = p.DocType.ToString(),
        DocNumber       = p.DocNumber,
        FirstName       = p.FirstName,
        LastName        = p.LastName,
        BirthDate       = p.BirthDate,
        Age             = p.Age,
        BiologicalSex   = p.BiologicalSex.ToString(),
        ContactPhone    = p.ContactPhone,
        ContactEmail    = p.ContactEmail,
        DivipolaMunCode = p.DivipolaMunCode,
        DivipolaDeptCode = p.DivipolaDeptCode,
        CreatedAt       = p.CreatedAt,
        UpdatedAt       = p.UpdatedAt
    };
}
