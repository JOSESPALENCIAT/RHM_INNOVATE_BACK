using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RHM.Application.Interfaces;
using RHM.Domain.Interfaces;
using RHM.Infrastructure.Persistence;
using RHM.Infrastructure.Repositories;
using RHM.Infrastructure.Services;

namespace RHM.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Azure SQL / EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("AzureSql"),
                b => b.MigrationsAssembly("RHM.Infrastructure")));

        // MongoDB
        services.AddSingleton<MongoDbContext>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        // HttpClient factory (used by WebhookService)
        services.AddHttpClient("n8n");

        // Infrastructure Services
        services.AddScoped<JwtService>();
        services.AddScoped<QrService>();
        services.AddScoped<IWebhookService, WebhookService>();

        // Application Services (implemented in Infrastructure to access EF/Mongo)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<IFormResponseService, FormResponseService>();
        services.AddScoped<IRiasCardService, RiasCardService>();

        return services;
    }
}
