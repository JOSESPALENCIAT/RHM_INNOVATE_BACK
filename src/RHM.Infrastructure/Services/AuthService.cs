using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RHM.Application.DTOs.Auth;
using RHM.Application.Interfaces;
using RHM.Domain.Entities;
using RHM.Domain.Enums;
using RHM.Domain.Interfaces;
using RHM.Infrastructure.Persistence;
using RHM.Infrastructure.Services;
using RHM.Shared.Helpers;

namespace RHM.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ITenantRepository _tenantRepo;
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepo,
        ITenantRepository tenantRepo,
        AppDbContext context,
        JwtService jwtService,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _tenantRepo = tenantRepo;
        _context = context;
        _jwtService = jwtService;
        _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("La cuenta está desactivada.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        return await BuildLoginResponseAsync(user);
    }

    public async Task<LoginResponseDto> RegisterTenantAsync(RegisterTenantDto request)
    {
        var slug = string.IsNullOrEmpty(request.Slug)
            ? SlugHelper.Generate(request.TenantName)
            : request.Slug;

        if (await _tenantRepo.SlugExistsAsync(slug))
            throw new InvalidOperationException($"El slug '{slug}' ya está en uso.");

        var tenant = await _tenantRepo.CreateAsync(new Tenant
        {
            Name = request.TenantName,
            Slug = slug,
            ContactEmail = request.ContactEmail,
            IsActive = true
        });

        var user = await _userRepo.CreateAsync(new User
        {
            TenantId = tenant.Id,
            Email = request.ContactEmail,
            FirstName = request.AdminFirstName,
            LastName = request.AdminLastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            Role = UserRole.AccountAdmin,
            IsActive = true
        });

        user.Tenant = tenant;
        return await BuildLoginResponseAsync(user);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(r => r.User).ThenInclude(u => u.Tenant)
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow)
            ?? throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        return await BuildLoginResponseAsync(token.User);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<LoginResponseDto> BuildLoginResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var refreshDays = int.Parse(_config["Jwt:RefreshExpiryDays"] ?? "7");

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
        });
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60") * 60,
            User = new UserSessionDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                TenantId = user.TenantId,
                TenantName = user.Tenant?.Name ?? string.Empty
            }
        };
    }
}
