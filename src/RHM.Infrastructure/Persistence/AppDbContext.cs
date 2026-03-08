using Microsoft.EntityFrameworkCore;
using RHM.Domain.Entities;
using RHM.Domain.Enums;
using RHM.Infrastructure.Persistence.Seed;

namespace RHM.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DivipolaCode> DivipolaCodes => Set<DivipolaCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Slug).IsUnique();
            e.HasIndex(t => t.ContactEmail).IsUnique();
            e.Property(t => t.Name).HasMaxLength(200).IsRequired();
            e.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            e.Property(t => t.ContactEmail).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.MonthlyAmount).HasColumnType("decimal(10,2)");
            e.HasOne(s => s.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.Property(r => r.Token).HasMaxLength(500).IsRequired();
            e.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Master Patient Index ---
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);

            // Llave natural Colombia: TenantId + TipoDoc + NumDoc → única por tenant
            e.HasIndex(p => new { p.TenantId, p.DocType, p.DocNumber }).IsUnique();

            e.Property(p => p.DocNumber).HasMaxLength(20).IsRequired();
            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            e.Property(p => p.ContactPhone).HasMaxLength(20);
            e.Property(p => p.ContactEmail).HasMaxLength(256);
            e.Property(p => p.DivipolaMunCode).HasMaxLength(5);
            e.Property(p => p.DivipolaDeptCode).HasMaxLength(2);

            e.Property(p => p.DocType)
                .HasConversion<string>()
                .HasMaxLength(5);

            e.Property(p => p.BiologicalSex)
                .HasConversion<string>()
                .HasMaxLength(15);

            // Age es calculado, no se persiste
            e.Ignore(p => p.Age);

            e.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Catálogo DIVIPOLA ---
        modelBuilder.Entity<DivipolaCode>(e =>
        {
            e.HasKey(d => d.MunCode);
            e.Property(d => d.MunCode).HasMaxLength(5).IsRequired();
            e.Property(d => d.DeptCode).HasMaxLength(2).IsRequired();
            e.Property(d => d.Departamento).HasMaxLength(100).IsRequired();
            e.Property(d => d.Municipio).HasMaxLength(100).IsRequired();
            e.Property(d => d.MunicipioNormalized).HasMaxLength(100).IsRequired();
            e.HasIndex(d => d.MunicipioNormalized);
            e.HasIndex(d => d.DeptCode);

            // Seed con los principales municipios de Colombia
            e.HasData(DivipolaSeed.GetData());
        });
    }
}
