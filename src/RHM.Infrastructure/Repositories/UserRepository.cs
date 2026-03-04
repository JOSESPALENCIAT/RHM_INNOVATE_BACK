using Microsoft.EntityFrameworkCore;
using RHM.Domain.Entities;
using RHM.Domain.Interfaces;
using RHM.Infrastructure.Persistence;

namespace RHM.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.Include(u => u.Tenant).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.Include(u => u.Tenant).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId) =>
        await _context.Users.Where(u => u.TenantId == tenantId).ToListAsync();

    public async Task<int> CountByTenantAsync(Guid tenantId) =>
        await _context.Users.CountAsync(u => u.TenantId == tenantId && u.IsActive);

    public async Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
