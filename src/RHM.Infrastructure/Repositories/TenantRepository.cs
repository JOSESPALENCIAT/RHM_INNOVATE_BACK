using Microsoft.EntityFrameworkCore;
using RHM.Domain.Entities;
using RHM.Domain.Interfaces;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context) => _context = context;

    public async Task<Tenant?> GetByIdAsync(Guid id) =>
        await _context.Tenants.FindAsync(id);

    public async Task<Tenant?> GetBySlugAsync(string slug) =>
        await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);

    public async Task<IEnumerable<Tenant>> GetAllAsync() =>
        await _context.Tenants.ToListAsync();

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        tenant.Id = Guid.NewGuid();
        tenant.CreatedAt = DateTime.UtcNow;
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task<Tenant> UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SlugExistsAsync(string slug) =>
        await _context.Tenants.AnyAsync(t => t.Slug == slug);
}
