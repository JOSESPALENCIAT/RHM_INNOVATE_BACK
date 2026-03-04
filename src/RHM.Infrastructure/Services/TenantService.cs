using Microsoft.EntityFrameworkCore;
using RHM.Application.DTOs.Tenants;
using RHM.Application.Interfaces;
using RHM.Domain.Interfaces;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly AppDbContext _context;

    public TenantService(ITenantRepository tenantRepo, AppDbContext context)
    {
        _tenantRepo = tenantRepo;
        _context = context;
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await _context.Tenants
            .Where(t => t.Slug != "rhm-system")
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                ContactEmail = t.ContactEmail,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UserCount = _context.Users.Count(u => u.TenantId == t.Id && u.IsActive)
            })
            .ToListAsync();

        return tenants;
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id)
    {
        var t = await _tenantRepo.GetByIdAsync(id);
        if (t is null) return null;

        var userCount = await _context.Users.CountAsync(u => u.TenantId == id && u.IsActive);
        return new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            ContactEmail = t.ContactEmail,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UserCount = userCount
        };
    }

    public async Task<TenantDto> UpdateAsync(Guid id, TenantDto dto)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Tenant no encontrado.");

        tenant.Name = dto.Name;
        tenant.ContactEmail = dto.ContactEmail;
        tenant.IsActive = dto.IsActive;
        await _tenantRepo.UpdateAsync(tenant);

        return (await GetByIdAsync(id))!;
    }

    public async Task SetActiveAsync(Guid id, bool isActive)
    {
        var tenant = await _tenantRepo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Tenant no encontrado.");

        tenant.IsActive = isActive;
        await _tenantRepo.UpdateAsync(tenant);
    }
}
