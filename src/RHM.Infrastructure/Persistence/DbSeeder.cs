using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RHM.Domain.Entities;
using RHM.Domain.Enums;

namespace RHM.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config  = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Leer credenciales del superadmin desde configuración
        var email     = config["SuperAdmin:Email"]    ?? "superadmin@rhminnovate.com";
        var password  = config["SuperAdmin:Password"] ?? throw new InvalidOperationException("SuperAdmin:Password no está configurado.");
        var firstName = config["SuperAdmin:FirstName"] ?? "Super";
        var lastName  = config["SuperAdmin:LastName"]  ?? "Admin";

        // No crear si ya existe
        if (await context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin))
        {
            logger.LogInformation("SuperAdmin ya existe — seed omitido.");
            return;
        }

        // Tenant del sistema (no es un cliente, es interno)
        var systemTenant = new Tenant
        {
            Id           = Guid.NewGuid(),
            Name         = "RHM System",
            Slug         = "rhm-system",
            ContactEmail = email,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };

        var superAdmin = new User
        {
            Id           = Guid.NewGuid(),
            TenantId     = systemTenant.Id,
            Email        = email,
            FirstName    = firstName,
            LastName     = lastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role         = UserRole.SuperAdmin,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };

        context.Tenants.Add(systemTenant);
        context.Users.Add(superAdmin);
        await context.SaveChangesAsync();

        logger.LogInformation("SuperAdmin creado: {Email}", email);
    }
}
